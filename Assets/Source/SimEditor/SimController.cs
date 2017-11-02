using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;
using SharpNeat.Core;
using SharpNeat.Phenomes;
using SharpNeat.Genomes.Neat;
using UnityEngine.UI;
using System.Linq;

public class SimController : MonoBehaviour
{
    public GameObject tilePref;
    public GameObject warrPref;
    public GameObject projectilePref;
    public GameObject obstaclePref;
    public GameObject playerPref;

    [HideInInspector] public GameObject blocksParent;
    [HideInInspector] public GameObject warriorsParent;
    [HideInInspector] public GameObject projectilesParent;
    [HideInInspector] public GameObject playersParent;

    WarriorController[] warriors;//cached for performance

    System.Random random;

    int warriorsTotal;

    [HideInInspector] public int height;
    [HideInInspector] public int width;

    private int playerAmount;

    public Assets.Source.CoEvaluator coEvaluator;

    public static StructureOfWarrior defaultWarStrct = new StructureOfWarrior()
    {
        blood = 1000,
        str = new List<Limb>(){
            new Limb("default", 50f, 0.01f, false),
            new Limb("default", 50f, 0.01f, false),
            new Limb("default", 50f, 0.01f, false)
            }
    };

    //Assets.Source.BattleExperiment experiment;
    // Use this for initialization
    void Start()
    {
        Debug.Log("Main thread is " + Thread.CurrentThread.ManagedThreadId);
        Debug.Assert(warriorsTotal % 2 == 0);
    }

    void Update()
    {
        ProcessUserInput();
    }

    public void Initialize()
    {
        Debug.Log("Initializing sim");
        blocksParent = new GameObject("blocksParent");
        warriorsParent = new GameObject("warriorsParent");
        projectilesParent = new GameObject("projectilesParent");
        playersParent = new GameObject("playersParent");

        CreateNewMap(width, height);
    }

    public void ResetPlayers()
    {
        foreach (Transform item in playersParent.transform)
        {
            var scrpt = item.GetComponent<PlayerController>();
            warriorsTotal -= scrpt.AmoutOfWarriorsOwns;
            playerAmount -= 1;

            Destroy(item.gameObject);
        }
    }


    public void AddPlayer(int team, int amoutOfWarriorsOwns, bool freeze)
    {
        var pl = Instantiate(playerPref, playersParent.transform);
        var scrpt = pl.GetComponent<PlayerController>();
        scrpt.Initialize(new Vector2(4, 4), amoutOfWarriorsOwns, team, freeze);
        warriorsTotal += scrpt.AmoutOfWarriorsOwns;
        playerAmount += 1;
    }

    public void InitPlayers()
    {
        Debug.Log("Initializing players");

        coEvaluator = new Assets.Source.CoEvaluator(playerAmount);
    }


    public bool userWantsToRun;
    public IEnumerator StartPerformingGenerations()
    {
        Debug.Assert(!userWantsToRun);
        if (!userWantsToRun)
        {
            userWantsToRun = true;
            for (int i = 0; ; i++)
            {
                if (userWantsToRun == false)
                {
                    break;
                }

                foreach (Transform child in playersParent.transform)
                {
                    StartCoroutine(child.GetComponent<PlayerController>().BeginDoingOneGeneration());
                }

                for (int j = 0; ; j++)
                {
                    if (coEvaluator.EvalRequested)//evaluation requested
                    {
                        Debug.Log($"Waited for evaluation's request for {j} frames");
                        break;
                    }
                    yield return null;
                }
                yield return StartCoroutine(MutualEvaluation());
                yield return null;
            }
        }
    }

    float timeScaleEvaluation = 1;
    public void SetTimeScaleEvaluation(float toSet)
    {
        timeScaleEvaluation = toSet;
    }

    private void DeleteMap()
    {
        foreach (Transform child in blocksParent.transform)
        {
            Destroy(child.gameObject);
        }
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

    #region Methods for evaluating
    //bool mutualEvalCoroutineStarted;

    /*
    public IEnumerator StartEvaluationOfPlayersGenomes(List<NeatGenome> genomesList, PlayerController playerSubmitter)
    {
        mutualEvalCoroutineStarted = false;
        

        for (int i = 0; ; i++)
        {
            if (playerNetsDict.Count == playerAmount)//if all players submitted
            {
                if (mutualEvalCoroutineStarted == false)//only 1 player starts coroutine
                {
                    Debug.Log("All players have submitted their genome lists");
                    mutualEvalCoroutineStarted = true;
                    StartCoroutine(MutualEvaluation());
                }
                break;//all players break
            }

            yield return null;
        }
        for (int i = 0; ; i++)//wait for MutualEvaluation coroutine to finish
        {
            if (!mutualEvaluatingFlag && i > 0)//wait several frames before break
            {
                break;
            }
            yield return null;
        }
    }*/


    int evaluationCount = 0;
    public bool mutualEvaluatingFlag;
    IEnumerator MutualEvaluation()
    {
        Debug.Assert(mutualEvaluatingFlag == false);//can't start if already started
        mutualEvaluatingFlag = true;

        foreach (var playerNets in coEvaluator.playerNetsDict)
        {
            SpawnWarriorsAndAssignBoxes(
                playerNets.Key.transform.position,
                playerNets.Value.Keys.ToList(),
                playerNets.Key,
                playerNets.Key.AmoutOfWarriorsOwns,
                playerNets.Key.Team);
            Debug.Log($"{playerNets.Key.Team} team");
        }

        for (int i = 0; i < HelperConstants.evaluationsPerGeneration; i++)//HelperConstants.evaluationsPerGeneration evaluations
        {
            Debug.Log($"Setting timescale to {timeScaleEvaluation}");
            Time.timeScale = timeScaleEvaluation;
            ResetWarriorsToTheirDefaults();
            yield return StartCoroutine(Evaluate());
            Time.timeScale = 1;
        }
        Debug.Log("Mutual evaluation finished");
        AssignEvaluatedFitnessesToGenomes();
        mutualEvaluatingFlag = false;
        evaluationCount++;
        coEvaluator.evalFinished = true;
    }

    #region Small helper functions
    private void AssignEvaluatedFitnessesToGenomes()
    {
        Dictionary<IBlackBox, NeatGenome> merged = new Dictionary<IBlackBox, NeatGenome>();
        foreach (var keyVal in coEvaluator.playerNetsDict)
        {
            Helpers.CopyElements(keyVal.Value, merged);
        }
        foreach (var warrior in warriors)
        {
            merged[warrior.ai.network].EvaluationInfo.SetFitness(Math.Max(warrior.Fitness, 0));
        }
        DestroyWarriors();
    }

    private void DestroyWarriors()
    {
        foreach (Transform item in warriorsParent.transform)
        {
            Destroy(item.gameObject);
        }
    }

    private void ResetWarriorsToTheirDefaults()
    {
        foreach (var warrior in warriors)
        {
            warrior.Revive();
        }
    }

    private void SpawnWarriorsAndAssignBoxes(Vector2 pos, IList<IBlackBox> boxes, PlayerController pla, int amount, int team)
    {
        Debug.Assert(boxes.Count == amount);
        Debug.Assert(team >= 1);
        for (int i = 0; i < amount; i++)
        {
            var w = CreateNewWarrior(pos, defaultWarStrct, team, new NeuralAI(HelperConstants.totalAmountOfSensors, HelperConstants.totalAmountOfOutputsOfNet, random, boxes[i]), pla);
        }
        warriors = warriorsParent.GetComponentsInChildren<WarriorController>();
    }
    #endregion

    bool evaluatingFlag;
    int ticksForEvalMax = HelperConstants.ticksPerEvaluation;
    int ticksPassedFromStartEval = 0;
    private IEnumerator Evaluate()
    {
        Debug.Assert(!evaluatingFlag);//can't start evaluating if already evaluating
        evaluatingFlag = true;

        foreach (var player in playersParent.GetComponentsInChildren<PlayerController>())
        {
            StartCoroutine(player.TickTrain());
        }

        while (ticksPassedFromStartEval < ticksForEvalMax)
        {
            Tick();
            ticksPassedFromStartEval++;
            if (ticksPassedFromStartEval % 200 == 0) Debug.Log(ticksPassedFromStartEval + " ticks passed");
            yield return new WaitForFixedUpdate();
        }
        Debug.Log("Evaluation finished");
        ticksPassedFromStartEval = 0;
        evaluatingFlag = false;
    }

    public void Tick()
    {
        var projectiles = projectilesParent.GetComponentsInChildren<ProjectileController>();
        foreach (var proj in projectiles)
        {
            proj.Tick();
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
        GameObject newObj;
        if (empty)
        {
            newObj = Instantiate(tilePref, blocksParent.transform);
        }
        else
        {
            newObj = Instantiate(obstaclePref, blocksParent.transform);
        }
        var scrpt = newObj.GetComponent<BlockController>();
        scrpt.transform.position = pos;

        return scrpt;
    }

    private WarriorController CreateNewWarrior(Vector2 pos, StructureOfWarrior str, int team, NeuralAI ai, PlayerController pla)
    {
        var newObj = Instantiate(warrPref, warriorsParent.transform);

        var scrpt = newObj.GetComponent<WarriorController>();
        scrpt.Initialize(pos, str, team, ai, pla);

        return scrpt;
    }

    public ProjectileController CreateNewProjectile(Vector2 start, Vector2 direction, float damage, WarriorController shooter)
    {
        var newObj = Instantiate(projectilePref, projectilesParent.transform);

        var scrpt = newObj.GetComponent<ProjectileController>();
        scrpt.Initialize(start, direction, damage, shooter);

        return scrpt;
    }
    #endregion

    #region User input
    public Text textToDisplay;
    WarriorController currentlySelectedWarr;
    private void ProcessUserInput()
    {
        if (textToDisplay != null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                var worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var coll = Physics2D.OverlapCircle(worldPoint, 0.5f, LayerMask.GetMask("Warrior"));

                if (coll)
                {
                    currentlySelectedWarr = coll.GetComponent<WarriorController>();
                }
                else
                {
                    currentlySelectedWarr = null;
                }
            }
            if (currentlySelectedWarr != null)
                textToDisplay.text = currentlySelectedWarr.toShowOnGui;
        }
    }


    #endregion
}