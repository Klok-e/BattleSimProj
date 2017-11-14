using SharpNeat.Phenomes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Warrior;

namespace BattleMode
{
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager battleManagerInst;

        public GameObject tilePref;
        public GameObject warrPref;
        public GameObject projectilePref;
        public GameObject obstaclePref;
        public GameObject playerPref;

        [HideInInspector] public GameObject blocksParent;
        [HideInInspector] public GameObject warriorsParent;
        [HideInInspector] public GameObject projectilesParent;
        [HideInInspector] public GameObject playersParent;

        public static StructureOfWarrior defaultWarStrct = new StructureOfWarrior()
        {
            blood = 1000,
            str = new List<Limb>(){
            new Limb("default", 50f, 0.01f, false),
            new Limb("default", 50f, 0.01f, false),
            new Limb("default", 50f, 0.01f, false)
            }
        };

        private System.Random random;

        private void Start()
        {
            battleManagerInst = this;

            new StartBattleSettings();//init singleton

            InitializeEverything();
        }

        private void InitializeEverything()
        {
            InitializeMap();
            InitPlayers();
        }

        private void Update()
        {
            ProcessUserInput();
        }

        private void ResetWarriorsToTheirDefaults()
        {
            var dict = new Dictionary<APlayerController, float[]>();
            foreach (var warrior in allWarriors)
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

        private IEnumerator GameCoroutine()
        {
            CreateWarriors();
            ResetWarriorsToTheirDefaults();
            bool gameFinished = false;
            while (gameFinished == false)
            {
                gameFinished = Tick();
                yield return new WaitForFixedUpdate();
            }
            DestroyWarriors();
            battleStarted = false;
        }

        public bool Tick()
        {
            var projectiles = projectilesParent.GetComponentsInChildren<ProjectileController>();
            foreach (var proj in projectiles)
            {
                proj.Tick();
            }

            var warriorsAlive = new Dictionary<int, int>();//team, amount
            foreach (var warrior in allWarriors)
            {
                if (!warriorsAlive.ContainsKey(warrior.team))
                {
                    warriorsAlive.Add(warrior.team, 0);
                }
                if (warrior.gameObject.activeSelf)
                {
                    warriorsAlive[warrior.team] += 1;
                    if (warrior.currentAction != null)
                    {
                        if (!warrior.currentAction.isFinished)
                            warrior.Tick();
                        else
                            warrior.Tick(warrior.ChooseAction());
                    }
                }
            }
            int deadTeams = 0;
            foreach (var keyVal in warriorsAlive)
            {
                if (keyVal.Value <= 0)
                {
                    deadTeams++;
                }
            }
            if (deadTeams >= warriorsAlive.Count - 1)//all lost
            {
                return true;
            }
            return false;
        }

        private WarriorController[] allWarriors;

        private void CreateWarriors()
        {
            foreach (Transform item in playersParent.transform)
            {
                var pl = item.GetComponent<BattlePlayer>();
                for (int i = 0; i < StartBattleSettings.singleton.warriorsTotal; i++)
                {
                    var w = CreateNewWarrior(pl.transform.position, defaultWarStrct, pl.Team,
                        new NeuralAI(HelperConstants.totalAmountOfSensors, HelperConstants.totalAmountOfOutputsOfNet, random, pl.phenomes[i]), pl);
                }
            }
            allWarriors = warriorsParent.GetComponentsInChildren<WarriorController>();
        }

        private void DestroyWarriors()
        {
            foreach (Transform item in warriorsParent.transform)
            {
                Destroy(item.gameObject);
            }
        }

        #region UI related methods

        private bool battleStarted;

        public void StartGame()
        {
            if (battleStarted == false)
            {
                battleStarted = true;
                StartCoroutine(GameCoroutine());
            }
        }

        private BattlePlayer currentlySelectedPlayer;

        public void ProcessUserInput()
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                var offsetVector = new Vector3(0, 0, 1);
                if (Input.GetMouseButtonDown(0))
                {
                    var worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    var coll = Physics2D.OverlapCircle(worldPoint, 0.5f, LayerMask.GetMask("PlayerFlags"));

                    if (coll)
                    {
                        if (currentlySelectedPlayer != null)
                        {
                            currentlySelectedPlayer.GetComponent<SpriteRenderer>().color = Color.white;
                        }
                        currentlySelectedPlayer = coll.GetComponent<BattlePlayer>();
                        currentlySelectedPlayer.GetComponent<SpriteRenderer>().color = Color.green;
                    }
                    else
                    {
                        if (currentlySelectedPlayer != null)
                        {
                            currentlySelectedPlayer.GetComponent<SpriteRenderer>().color = Color.white;
                        }
                        currentlySelectedPlayer = null;
                    }
                }
                if (battleStarted)
                {
                    if (Input.GetMouseButton(0) && currentlySelectedPlayer != null)
                    {
                        var worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                        currentlySelectedPlayer.transform.position = (Vector2)worldPoint;
                    }
                }
            }
        }

        #endregion UI related methods

        private void InitializeMap()
        {
            Debug.Log("Initializing sim");
            blocksParent = new GameObject("blocksParent");
            warriorsParent = new GameObject("warriorsParent");
            projectilesParent = new GameObject("projectilesParent");
            playersParent = new GameObject("playersParent");

            CreateNewMap(StartBattleSettings.singleton.width, StartBattleSettings.singleton.height);
        }

        private void InitPlayers()
        {
            var nets = StartBattleSettings.singleton.DecodeAllGenomes();
            AddPlayer(1, new Vector2(2, 2), nets[0]);
            AddPlayer(2, new Vector2(StartBattleSettings.singleton.width - 3, StartBattleSettings.singleton.height - 3), nets[1]);
        }

        public void CreateNewMap(int width, int height)
        {
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

        #region Create game object methods

        private void AddPlayer(int team, Vector2 pos, List<IBlackBox> nets)
        {
            var pl = Instantiate(playerPref, playersParent.transform);
            var scrpt = pl.GetComponent<BattlePlayer>();

            scrpt.Initialize(pos, team, nets);
        }

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

        private WarriorController CreateNewWarrior(Vector2 pos, StructureOfWarrior str, int team, NeuralAI ai, BattlePlayer pla)
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
    }
}
