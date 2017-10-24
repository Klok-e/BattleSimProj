using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;
using SharpNeat.Core;
using SharpNeat.Phenomes;
using SharpNeat.Genomes.Neat;
using UnityEngine.UI;

public class SimController : MonoBehaviour
{
    public static SimController simInstance;

    public GameObject tilePref;
    public GameObject warrPref;
    public GameObject bodyPref;
    public GameObject obstaclePref;

    List<BlockController> blocks;
    List<WarriorController> warriors;
    List<BodyController> bodies;

    System.Random random;

    int warriorsTotal = 30;

    public int height;
    public int width;

    public static StructureOfWarrior defaultWarStrct = new StructureOfWarrior()
    {
        blood = 1000,
        str = new List<Limb>(){
            new Limb("default", 50f, 0.01f, false),
            new Limb("default", 50f, 0.01f, false),
            new Limb("default", 50f, 0.01f, false)
            }
    };

    Assets.Source.BattleExperiment experiment;
    // Use this for initialization
    void Start()
    {
        Debug.Log("Main thread is " + System.Threading.Thread.CurrentThread.ManagedThreadId);

        Debug.Assert(warriorsTotal % 2 == 0);

        simInstance = this;
        blocks = new List<BlockController>();
        warriors = new List<WarriorController>();
        bodies = new List<BodyController>();

        CreateNewMap(width, height);
        experiment = CreateExperiment();

        Debug.Log(experiment._ea.RunState);

        SpawnWarriors(warriorsTotal);
    }

    void Update()
    {
        ProcessUserInput();
    }

    public bool userWantsToStart;
    public IEnumerator StartPerformingGenerations()
    {
        if (!userWantsToStart)
        {
            userWantsToStart = true;
            for (int i = 0; ; i++)
            {
                Time.timeScale = timeScaleEvaluation;
                if (userWantsToStart == false)
                {
                    break;
                }

                yield return StartCoroutine(experiment._ea.DoOneGeneration());
                if (i % 3 == 0)
                {
                    //save
                    experiment.SavePopulation("myPopBackUp.xml");
                }

                Time.timeScale = 1;
            }
        }
    }

    public void LoadPopulation(string filename)
    {
        var battleEvaluator = new Assets.Source.BattleEvaluator<NeatGenome>();

        experiment.CreateEvolutionAlgorithm(battleEvaluator, experiment.LoadPopulation(filename));
    }

    public void SavePopulation(string filename)
    {
        experiment.SavePopulation(filename);
    }

    float timeScaleEvaluation = 1;
    public void SetTimeScaleEvaluation(float toSet)
    {
        timeScaleEvaluation = toSet;
    }

    private void DeleteMap()
    {
        foreach (var item in blocks)
        {
            Destroy(item.gameObject);
        }
        blocks.Clear();
    }

    public void CreateNewMap(int width, int height)
    {
        this.width = width;
        this.height = height;
        DeleteMap();
        random = new System.Random();
        //generate map
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                var bl = CreateNewBlock(new Vector2(i, j), (i == 0 || i == width - 1 || j == 0 || j == height - 1) ? false : true);
            }
        }
        Debug.Log("Map created");
    }

    IGenomeDecoder<NeatGenome, IBlackBox> decoder;
    private Assets.Source.BattleExperiment CreateExperiment()
    {
        var experiment = new Assets.Source.BattleExperiment();

        var battleEvaluator = new Assets.Source.BattleEvaluator<NeatGenome>();

        decoder = experiment.CreateDecoder();
        experiment._ea = experiment.CreateEvolutionAlgorithm(battleEvaluator, decoder, warriorsTotal);
        Debug.Log("Experiment created");
        return experiment;
    }

    public IEnumerator StartEvaluatingGenomes(List<NeatGenome> list)
    {
        for (int i = 0; i < 3; i++)//evaluations for stable train
        {
            ResetWarriors(decoder, list);
            Debug.Log("Warriors reset");
            yield return StartCoroutine(Evaluate());
        }

        #region Log statistics and delete fitness
        float totalFitness = 0;
        float mxFitness = warriors[0].GetFitness();
        float totalComplexity = 0;
        float mxComplexity = (float)warriors[0].ai.genome.Complexity;
        foreach (var warr in warriors)
        {
            totalFitness += warr.GetFitness();
            mxFitness = Math.Max(mxFitness, warr.GetFitness());
            totalComplexity += (float)warr.ai.genome.Complexity;
            mxComplexity = Math.Max(mxComplexity, (float)warr.ai.genome.Complexity);

            warr.fitness=0;//delete fitness
        }
        Debug.Log(String.Format("Max fitness is {0}; Mean fitnes is {1}; Max complexity is {2}; Mean complexity is {3}",
            mxFitness,
            totalFitness / warriorsTotal,
            mxComplexity,
            totalComplexity / warriorsTotal));
        #endregion
    }

    #region Methods for evaluating
    private void SpawnWarriors(int warrTot)
    {
        for (int i = 0; i < warrTot; i++)
        {
            WarriorController w;
            if (i % 2 == 0)
            {
                w = CreateNewWarrior(new Vector2(1 + HelperConstants.warriorSpawnOffset, 1 + HelperConstants.warriorSpawnOffset),
                    defaultWarStrct, 1, new NeuralAI(HelperConstants.totalAmountOfSensors, HelperConstants.totalAmountOfOutputsOfNet, random));
            }
            else
            {
                w = CreateNewWarrior(new Vector2(width - 2 - HelperConstants.warriorSpawnOffset, height - 2 - HelperConstants.warriorSpawnOffset),
                    defaultWarStrct, 2, new NeuralAI(HelperConstants.totalAmountOfSensors, HelperConstants.totalAmountOfOutputsOfNet, random));
            }
        }
    }

    private void ResetWarriors(IGenomeDecoder<NeatGenome, IBlackBox> deco, List<NeatGenome> genomes)
    {
        for (int i = 0; i < warriorsTotal; i++)
        {
            var w = warriors[i];
            IBlackBox box = null;
            box = deco.Decode(genomes[i]);
            w.ai.SetNetworkAndGenome(box, genomes[i]);
            w.Revive();
            if (w.team == 1)
            {
                w.transform.position = new Vector2(1 + HelperConstants.warriorSpawnOffset, 1 + HelperConstants.warriorSpawnOffset);
            }
            else
            {
                w.transform.position = new Vector2(width - 2 - HelperConstants.warriorSpawnOffset, height - 2 - HelperConstants.warriorSpawnOffset);
            }
        }
    }

    int time;
    int fps = 100;
    int ticksForEvalMax = 1000;
    int ticksPassedFromStartEval = 0;
    private IEnumerator Evaluate()
    {
        while (ticksPassedFromStartEval < ticksForEvalMax)
        {
            Tick();
            ticksPassedFromStartEval++;
            if (ticksPassedFromStartEval % 200 == 0) Debug.Log(ticksPassedFromStartEval + " ticks passed");
            time = Environment.TickCount;

            yield return new WaitForFixedUpdate();
        }
        ticksPassedFromStartEval = 0;


    }

    public void Tick()
    {
        foreach (var body in bodies)
        {
            body.Tick();
        }

        foreach (var warrior in warriors)
        {
            if (warrior.currentAction != null && warrior.gameObject.activeSelf)
            {
                if (!warrior.currentAction.isFinished)
                    warrior.Tick();
                else
                    warrior.Tick(warrior.ChooseAction());
            }
        }
    }
    #endregion

    #region Create game object methods
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

    private WarriorController CreateNewWarrior(Vector2 pos, StructureOfWarrior str, int team, NeuralAI ai)
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
        scrpt.rotationSpeed = HelperConstants.warriorRotationSpeed;

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
    #endregion

    #region User input
    public Text textToDisplay;
    WarriorController currentlySelectedWarr;
    private void ProcessUserInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var coll = Physics2D.OverlapCircle(worldPoint, 0.5f, LayerMask.GetMask("Warrior"));

            if (coll)
            {
                currentlySelectedWarr = coll.GetComponent<WarriorController>();
                currentlySelectedWarr.userControlled = true;
            }
            else
            {
                if (currentlySelectedWarr != null)
                    currentlySelectedWarr.userControlled = false;
                currentlySelectedWarr = null;
            }
        }
        if (currentlySelectedWarr != null)
            textToDisplay.text = currentlySelectedWarr.toShowOnGui;
    }


    #endregion
}