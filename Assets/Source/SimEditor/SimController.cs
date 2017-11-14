using SharpNeat.Genomes.Neat;
using SharpNeat.Phenomes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Warrior;

namespace SimEditor
{
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

        private WarriorController[] warriors;//cached for performance

        private System.Random random;

        private int warriorsTotal;

        public int height;
        public int width;

        private int playerAmount;

        public CoEvaluator coEvaluator;

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
        private void Start()
        {
            Debug.Log("Main thread is " + Thread.CurrentThread.ManagedThreadId);
            Debug.Assert(warriorsTotal % 2 == 0);
        }

        private void Update()
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

            coEvaluator = new CoEvaluator(playerAmount);
        }

        [HideInInspector] public bool userWantsToRun;

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

        private float timeScaleEvaluation = 1;

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

        private int evaluationCount = 0;
        [HideInInspector] public bool mutualEvaluatingFlag;

        private IEnumerator MutualEvaluation()
        {
            Debug.Assert(mutualEvaluatingFlag == false);//can't start if already started
            mutualEvaluatingFlag = true;

            foreach (var playerNets in coEvaluator.playerNetsDict)
            {
                SpawnWarriorsAndAssignBoxes(
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
                merged[warrior.ai.network].EvaluationInfo.SetFitness(Math.Max(warrior.GetFitness(), 0));
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
            var dict = new Dictionary<APlayerController, float[]>();
            foreach (var warrior in warriors)
            {
                if (!dict.ContainsKey(warrior.PlayerOwner))
                {
                    dict.Add(warrior.PlayerOwner, new float[3] { 0, 1, 1 });//0 - shiftFromCenter, 1 - countShiftIsIncreasedMax, 2 - count
                }
                if (dict[warrior.PlayerOwner][2] <= 0)
                {
                    dict[warrior.PlayerOwner][0] += warrior.GetComponent<CircleCollider2D>().radius;
                    dict[warrior.PlayerOwner][1] *= 3;
                    dict[warrior.PlayerOwner][2] = dict[warrior.PlayerOwner][1];
                }
                var pos = warrior.PlayerOwner.transform.position + (Vector3)Helpers.RandomVector2() * dict[warrior.PlayerOwner][0];
                warrior.Revive(pos);
                dict[warrior.PlayerOwner][2]--;
            }
        }

        private void SpawnWarriorsAndAssignBoxes(IList<IBlackBox> boxes, PlayerController pla, int amount, int team)
        {
            Debug.Assert(boxes.Count == amount);
            Debug.Assert(team >= 1);

            for (int i = 0; i < amount; i++)
            {
                var w = CreateNewWarrior(
                    pla.transform.position,
                    defaultWarStrct,
                    team,
                    new NeuralAI(HelperConstants.totalAmountOfSensors, HelperConstants.totalAmountOfOutputsOfNet, random, boxes[i]), pla);
            }
            warriors = warriorsParent.GetComponentsInChildren<WarriorController>();
        }

        #endregion Small helper functions

        private bool evaluatingFlag;
        private int ticksForEvalMax = HelperConstants.ticksPerEvaluation;
        private int ticksPassedFromStartEval = 0;

        private IEnumerator Evaluate()
        {
            Debug.Assert(!evaluatingFlag);//can't start evaluating if already evaluating
            evaluatingFlag = true;

            var plrs = playersParent.GetComponentsInChildren<PlayerController>();

            ResetPlayersToDefault(plrs);

            while (ticksPassedFromStartEval < ticksForEvalMax)
            {
                Tick(plrs);
                ticksPassedFromStartEval++;
                yield return new WaitForFixedUpdate();
            }

            ResetPlayersToDefault(plrs);

            Debug.Log("Evaluation finished");
            ticksPassedFromStartEval = 0;
            evaluatingFlag = false;
        }

        private void ResetPlayersToDefault(PlayerController[] plrs)
        {
            foreach (var player in plrs)
                player.ResetToDefault();

            foreach (var proj in projectilesParent.GetComponentsInChildren<ProjectileController>())
                proj.Die();
        }

        public void Tick(PlayerController[] plrs)
        {
            var projectiles = projectilesParent.GetComponentsInChildren<ProjectileController>();

            foreach (var player in plrs)
                player.Tick();

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

        #endregion Methods for evaluating

        #region Create game object methods

        public BlockController CreateNewBlock(Vector2 pos, bool empty)
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

        public WarriorController CreateNewWarrior(Vector2 pos, StructureOfWarrior str, int team, NeuralAI ai, PlayerController pla)
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

        #endregion Create game object methods

        //TODO: make another script with all ui functions
        public void SetFitnessBonusForDyingFromEnemy(float num)
        {
            HelperConstants.fitnessBonusForDyingFromEnemy = num;
        }

        public void SetFitnessForKillingEnemy(float num)
        {
            HelperConstants.fitnessForKillingAnEnemy = num;
        }

        public void SetFitnessMultiplierForApproachingToFlag(float num)
        {
            HelperConstants.fitnessMultiplierForApproachingToFlag = num;
        }

        public void SetFitnessPenaltyForKillingAlly(float num)
        {
            HelperConstants.fitnessPenaltyForKillingAlly = num;
        }

        #region User input

        public Text textToDisplay;
        private WarriorController currentlySelectedWarr;

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
                    textToDisplay.text = currentlySelectedWarr.GetFitness().ToString();
            }
        }

        #endregion User input
    }
}
