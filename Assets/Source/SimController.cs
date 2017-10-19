using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;
using SharpNeat.Core;
using SharpNeat.Phenomes;
using SharpNeat.Genomes.Neat;

public class SimController : MonoBehaviour
{
    public GameObject tilePref;
    public GameObject warrPref;
    public GameObject bodyPref;
    public GameObject obstaclePref;

    List<BlockController> blocks;
    List<WarriorController> warriors;
    List<BodyController> bodies;

    System.Random random;

    int warriorsTotal = 10;

    public static StructureOfWarrior defaultWarStrct = new StructureOfWarrior()
    {
        blood = 1000,
        str = new List<Limb>(){
            new Limb("default", 50f, 0.01f, false),
            new Limb("default", 50f, 0.01f, false),
            new Limb("default", 50f, 0.01f, false)
            }
    };

    // Use this for initialization
    void Start()
    {
        Debug.Assert(warriorsTotal % 2 == 0);

        blocks = new List<BlockController>();
        warriors = new List<WarriorController>();
        bodies = new List<BodyController>();

        CreateNewMap(20, 20, warriorsTotal / 2);
    }

    int time;
    readonly int fps = 100;

    int timeFromStartOfEvalThreshold = 10000;
    int timeFromStartOfEval = Environment.TickCount;
    void Update()
    {
        if (Environment.TickCount - time > 1000 / fps)
        {
            if ((Environment.TickCount - timeFromStartOfEval > timeFromStartOfEvalThreshold))
            {
                timeFromStartOfEval = Environment.TickCount;
                PassGenerationAndAssign();
                SpawnWarriors(warriorsTotal / 2);
            }
            Tick();
            time = Environment.TickCount;
        }
    }

    public void Tick()
    {
        foreach (var body in bodies)
        {
            body.Tick();
        }

        foreach (var warrior in warriors)
        {
            if (warrior.currentAction != null)
            {
                if (!warrior.currentAction.isFinished)
                    warrior.Tick();
                else
                    warrior.Tick(warrior.ChooseAction());
            }
        }
    }

    private void SpawnWarriors(int warrOnEachSide)
    {
        #region DestroyAllWarriors
        foreach (var warr in warriors)
        {
            Destroy(warr.gameObject);
        }
        warriors.Clear();
        #endregion
        for (int i = 0; i < warrOnEachSide * 2; i++)
        {
            if (i % 2 == 0)
            {
                var w = CreateNewWarrior(new Vector2(1, 1), defaultWarStrct, 0, new NeuralAI(HelperConstants.totalAmountOfSensors, HelperConstants.totalAmountOfOutputsOfNet, random));
            }
            else
            {
                var w = CreateNewWarrior(new Vector2(5, 5), defaultWarStrct, 1, new NeuralAI(HelperConstants.totalAmountOfSensors, HelperConstants.totalAmountOfOutputsOfNet, random));
            }
        }
        AssignToWarriors();
    }

    private void CreateNewMap(int height, int width, int warrOnEachSide)
    {
        random = new System.Random();

        CreateExperiment();

        SpawnWarriors(warrOnEachSide);

        //generate map
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                var bl = CreateNewBlock(new Vector2(i, j), (i == 0 || i == height - 1 || j == 0 || j == width - 1) ? false : true);
            }
        }
    }

    private BlockController CreateNewBlock(Vector2 pos, bool empty)
    {
        if (empty)
        {
            var newObj = Instantiate(tilePref, transform);

            var scrpt = newObj.GetComponent<BlockController>();
            scrpt.transform.position = pos;

            blocks.Add(scrpt);
            return scrpt;
        }
        else
        {
            var newObj = Instantiate(obstaclePref, transform);

            var scrpt = newObj.GetComponent<BlockController>();
            scrpt.transform.position = pos;

            blocks.Add(scrpt);
            return scrpt;
        }
    }

    private WarriorController CreateNewWarrior(Vector2 pos, StructureOfWarrior str, int team, AAi ai)
    {
        var newObj = Instantiate(warrPref, transform);

        var scrpt = newObj.GetComponent<WarriorController>();
        scrpt.transform.position = pos;

        scrpt.ai = ai;
        scrpt.team = team;
        scrpt.bloodRemaining = str.blood;
        scrpt.limbs = new List<Limb>();
        foreach (var l in str.str)
        {
            scrpt.limbs.Add(l.Copy(scrpt));
        }
        scrpt.speed = HelperConstants.speedMultOfWa;

        warriors.Add(scrpt);
        return scrpt;
    }

    private BodyController CreateNewBody(Vector2 pos)
    {
        var newObj = Instantiate(bodyPref, transform);

        var scrpt = newObj.GetComponent<BodyController>();
        scrpt.transform.position = pos;

        bodies.Add(scrpt);
        return scrpt;
    }


    IGenomeDecoder<NeatGenome, IBlackBox> decoder;
    SharpNeat.EvolutionAlgorithms.NeatEvolutionAlgorithm<NeatGenome> algorithm;
    private void CreateExperiment()
    {
        var experiment = new Assets.Source.BattleExperiment();

        algorithm = experiment.CreateEvolutionAlgorithm(new Assets.Source.BattleEvaluator<NeatGenome>(), warriorsTotal);
        decoder = experiment.genomeDecoder;
        AssignToWarriors();
    }
    /// <summary>
    /// Implies that all genomes were already set a fitness
    /// </summary>
    private void PassGenerationAndAssign()
    {
        algorithm.PerformOneGeneration();
        AssignToWarriors();
    }

    private void AssignToWarriors()
    {
        if (warriors.Count > 0)
        {
            for (int i = 0; i < algorithm.GenomeList.Count; i++)
            {
                warriors[i].ai.network = decoder.Decode(algorithm.GenomeList[i]);
            }
        }
    }
}

