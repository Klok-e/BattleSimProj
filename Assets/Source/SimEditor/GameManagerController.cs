using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

namespace SimEditor
{
    public class GameManagerController : MonoBehaviour
    {
        public static GameManagerController inputManagerInstance;
        private bool isEditorMode;

        private LineRenderer lineRenderer;

        public GameObject[] disabledInBrushMode;
        public GameObject[] enabledOnlyInEditorMode;
        public GameObject saveLoadCurrPlayerPanel;

        public SimController simInst;

        public LoadMenu loadMenuInst;

        private void Start()
        {
            Application.runInBackground = true;
            SaveLoad.Load();
            loadMenuInst.Refresh();
            lineRenderer = GetComponent<LineRenderer>();
            inputManagerInstance = this;
            isEditorMode = true;

            simInst.Initialize();
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
            }
            else
            {
                SetThings(false);
                SetBrushThings(false);
            }
            ProcessUserInput();
        }

        private void SetThings(bool set)
        {
            saveLoadCurrPlayerPanel.SetActive(set);
            foreach (var item in enabledOnlyInEditorMode)
                item.SetActive(set);
        }

        private void SetBrushThings(bool set)
        {
            foreach (var item in disabledInBrushMode)
                item.SetActive(!set);//bcos disabled
        }

        private int amountOfWarriors;

        public void SetAmountOfWarriors(int amount)
        {
            amountOfWarriors = amount;
        }

        private int team = 1;

        public void AddPlayer(int amountOfW, bool freeze)
        {
            simInst.AddPlayer(team++, amountOfW, freeze);
        }

        public void ResetPlayers()
        {
            team = 1;
            simInst.ResetPlayers();
        }

        public void StartRunningGenerations()
        {
            simInst.InitPlayers();
            isEditorMode = false;
            StartCoroutine(simInst.StartPerformingGenerations());
        }

        private bool brushMode;

        public void SetBrushMode(bool toSet)
        {
            brushMode = toSet;
        }

        public bool AnyPlayersPresent()
        {
            if (team > 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void StopRunningGenerations()
        {
            StartCoroutine(SetEditorModeWhenEvalsFinished());
            simInst.userWantsToRun = false;
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

        private PlayerController currentlySelectedPlayer;

        private void ProcessUserInput()
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                var offsetVector = new Vector3(0, 0, 1);
                if (Input.GetMouseButtonDown(0) && !brushMode)//select flag
                {
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
                }
                if (isEditorMode)
                {
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
                    else
                    {
                        if (Input.GetMouseButton(0))
                        {
                            var worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                            var colls = Physics2D.OverlapCircleAll(worldPoint, HelperConstants.brushSize, LayerMask.GetMask("Tile"));

                            foreach (var item in colls)
                            {
                                simInst.CreateNewBlock(item.transform.position, false);
                                Destroy(item.gameObject);
                            }
                        }
                        if (Input.GetMouseButton(1))
                        {
                            var worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                            var colls = Physics2D.OverlapCircleAll(worldPoint, HelperConstants.brushSize, LayerMask.GetMask("Obstacle"));

                            foreach (var item in colls)
                            {
                                simInst.CreateNewBlock(item.transform.position, true);
                                Destroy(item.gameObject);
                            }
                        }
                    }
                }
            }
        }
    }
}
