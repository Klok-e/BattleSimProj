using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using Warrior;

namespace SimEditor
{
    public class GameManagerController : MonoBehaviour
    {
        public static GameManagerController inputManagerInstance;

        [HideInInspector] public SimController simInst;
        private bool brushMode;
        private PlayerController currentlySelectedPlayer;
        private WarriorController currentlySelectedWarr;
        [SerializeField] private GameObject[] disabledInBrushMode;
        [SerializeField] private GameObject[] enabledOnlyInEditorMode;

        private bool isEditorMode;
        private LineRenderer lineRenderer;
        [SerializeField] private LoadMenu loadMenuInst;
        [SerializeField] private GameObject saveLoadCurrPlayerPanel;
        [SerializeField] private SelectedWarriorStatsPanel selectedWarrStPanel;
        private int teamCount = 1;

        public void AddPlayer(int amountOfW, bool freeze)
        {
            simInst.AddPlayer(teamCount++, amountOfW, freeze);
        }

        public bool AnyPlayersPresent()
        {
            if (teamCount > 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void LoadPopToSelectedPlayer(string name)
        {
            if (currentlySelectedPlayer != null)
            {
                currentlySelectedPlayer.LoadPopulation(name);
            }
            else
            {
                Debug.Log("Couldn't load: player not selected");
            }
        }

        public void ResetPlayers()
        {
            teamCount = 1;
            simInst.ResetPlayers();
        }

        public void SaveCurrentPlayer(string name)
        {
            if (currentlySelectedPlayer != null)
            {
                currentlySelectedPlayer.SavePopulation(name);
                loadMenuInst.Refresh();
            }
            else
            {
                Debug.Log("Couldn't save");
            }
        }

        public void SetBrushMode(bool toSet)
        {
            brushMode = toSet;
        }

        public void StartRunningGenerations()
        {
            simInst.InitPlayers();
            isEditorMode = false;
            StartCoroutine(simInst.StartPerformingGenerations());
        }

        public void StopRunningGenerations()
        {
            StartCoroutine(SetEditorModeWhenEvalsFinished());
            simInst.userWantsToRun = false;
        }

        private void ProcessUserInput()
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                var offsetVector = new Vector3(0, 0, 1);
                if (Input.GetMouseButtonDown(0) && !brushMode)//select flag
                {
                    #region Select flag

                    var worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    var coll = Physics2D.OverlapCircle(worldPoint, 0.5f, LayerMask.GetMask("PlayerFlags"));

                    if (coll)
                    {
                        if (currentlySelectedPlayer != null)
                        {
                            currentlySelectedPlayer.GetComponent<SpriteRenderer>().color = Color.white;
                        }
                        currentlySelectedPlayer = coll.GetComponent<PlayerController>();
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

                    #endregion Select flag

                    #region Select warrior

                    worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    coll = Physics2D.OverlapCircle(worldPoint, 0.5f, LayerMask.GetMask(HelperConstants.warriorTag));

                    if (coll)
                    {
                        currentlySelectedWarr = coll.GetComponent<WarriorController>();

                        selectedWarrStPanel.DrawGenome(simInst.coEvaluator.playerNetsDict[(PlayerController)currentlySelectedWarr.PlayerOwner][currentlySelectedWarr.ai.network]);
                    }
                    else
                    {
                        currentlySelectedWarr = null;
                    }

                    #endregion Select warrior
                }
                if (isEditorMode)
                {
                    #region Not brush mode things

                    if (!brushMode)
                    {
                        if (Input.GetMouseButton(0) && currentlySelectedPlayer != null)//set position
                        {
                            var worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                            currentlySelectedPlayer.transform.position = (Vector2)worldPoint;
                            currentlySelectedPlayer.pointsToVisitDuringTraining.Clear();
                            currentlySelectedPlayer.defaultPos = (worldPoint);
                        }
                        if (Input.GetMouseButtonDown(1) && currentlySelectedPlayer != null)
                        {
                            var worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                            currentlySelectedPlayer.pointsToVisitDuringTraining.Add(new Vector3(worldPoint.x, worldPoint.y) - offsetVector);
                        }
                        if (Input.GetMouseButtonDown(2) && currentlySelectedPlayer != null)//reset points
                        {
                            currentlySelectedPlayer.pointsToVisitDuringTraining.Clear();
                        }
                    }

                    #endregion Not brush mode things

                    #region Brush mode things

                    else
                    {
                        #region Paint

                        if (Input.GetMouseButton(0))
                        {
                            var worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                            var colls = Physics2D.OverlapCircleAll(worldPoint, HelperConstants.brushSize, LayerMask.GetMask(HelperConstants.tileTag));

                            foreach (var item in colls)
                            {
                                simInst.CreateNewBlock(item.transform.position, false);
                                Destroy(item.gameObject);
                            }
                        }

                        #endregion Paint

                        #region Erase

                        if (Input.GetMouseButton(1))
                        {
                            var worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                            var colls = Physics2D.OverlapCircleAll(worldPoint, HelperConstants.brushSize, LayerMask.GetMask(HelperConstants.obstacleTag));

                            foreach (var item in colls)
                            {
                                simInst.CreateNewBlock(item.transform.position, true);
                                Destroy(item.gameObject);
                            }
                        }

                        #endregion Erase
                    }

                    #endregion Brush mode things
                }
            }
        }

        private void RefreshWarriorView()
        {
            selectedWarrStPanel.RefreshCells(currentlySelectedWarr.data, currentlySelectedWarr.prediction, (float)currentlySelectedWarr.GetFitness());
        }

        /// <summary>
        /// false - enable everything not included in brush mode
        /// </summary>
        /// <param name="set"></param>
        private void SetBrushThings(bool set)
        {
            foreach (var item in disabledInBrushMode)
                item.SetActive(!set);//bcos disabled
        }

        private IEnumerator SetEditorModeWhenEvalsFinished()
        {
            while (true)
            {
                if (!simInst.mutualEvaluatingFlag)
                {
                    isEditorMode = true;
                    break;
                }
                yield return null;
            }
        }

        /// <summary>
        /// false - disable everything not included in evolution mode
        /// </summary>
        /// <param name="set"></param>
        private void SetThings(bool set)
        {
            saveLoadCurrPlayerPanel.SetActive(set);
            foreach (var item in enabledOnlyInEditorMode)
                item.SetActive(set);
        }

        private void SetWarriorViewUI(bool set)
        {
            selectedWarrStPanel.gameObject.SetActive(set);
        }

        private void Start()
        {
            simInst = GetComponent<SimController>();

            Application.runInBackground = true;
            SaveLoad.Load();
            loadMenuInst.Refresh();
            lineRenderer = GetComponent<LineRenderer>();
            inputManagerInstance = this;
            isEditorMode = true;

            simInst.Initialize();
            selectedWarrStPanel.Initialize();
        }

        // Update is called once per frame
        private void Update()
        {
            if (isEditorMode)
            {
                if (!brushMode)
                {
                    SetBrushThings(false);
                    if (currentlySelectedPlayer != null)
                    {
                        saveLoadCurrPlayerPanel.SetActive(true);
                    }
                    else
                    {
                        saveLoadCurrPlayerPanel.SetActive(false);
                    }
                    foreach (var item in enabledOnlyInEditorMode)
                        item.SetActive(true);
                }
                else
                {
                    SetThings(false);
                    SetBrushThings(true);
                }
                SetWarriorViewUI(false);
            }
            else
            {
                SetThings(false);
                SetBrushThings(false);
                if (currentlySelectedWarr != null)
                {
                    SetWarriorViewUI(true);
                    RefreshWarriorView();
                }
                else
                {
                    SetWarriorViewUI(false);
                }
            }
            ProcessUserInput();
        }
    }
}
