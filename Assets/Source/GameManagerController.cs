using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

public class GameManagerController : MonoBehaviour
{
    public static GameManagerController inputManagerInstance;
    bool isEditorMode;

    LineRenderer lineRenderer;

    public GameObject editorPanel;
    public GameObject saveLoadCurrPlayerPanel;

    public SimController simInst;

    public LoadMenu loadMenuInst;

    private void Start()
    {
        SaveLoad.Load();
        loadMenuInst.Refresh();
        lineRenderer = GetComponent<LineRenderer>();
        inputManagerInstance = this;
        isEditorMode = true;
        SimController.simInstance = simInst;

        SimController.simInstance.Initialize();
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
            if (!SaveLoad.savedGames.Contains(name))
            {
                SaveLoad.Save(name);
            }
            loadMenuInst.Refresh();
            currentlySelectedPlayer.SavePopulation(name);
        }
        else
        {
            Debug.Log("Couldn't save");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isEditorMode)
        {
            if (currentlySelectedPlayer!=null)
            {
                saveLoadCurrPlayerPanel.SetActive(true);
            }
            else
            {
                saveLoadCurrPlayerPanel.SetActive(false);
            }
            editorPanel.SetActive(true);
        }
        else
        {
            saveLoadCurrPlayerPanel.SetActive(false);
            editorPanel.SetActive(false);
        }
        ProcessUserInput();
    }

    int amountOfWarriors;
    public void SetAmountOfWarriors(int amount)
    {
        amountOfWarriors = amount;
    }

    int team = 1;
    public void AddPlayer(int amountOfW)
    {
        SimController.simInstance.AddPlayer(team++, amountOfW);
    }
    public void ResetPlayers()
    {
        team = 1;
        SimController.simInstance.ResetPlayers();
    }

    public void StartRunningGenerations()
    {
        SimController.simInstance.InitPlayers();
        isEditorMode = false;
        StartCoroutine(SimController.simInstance.StartPerformingGenerations());
    }
    public void StopRunningGenerations()
    {
        isEditorMode = true;
        SimController.simInstance.userWantsToRun = false;
    }

    PlayerController currentlySelectedPlayer;
    void ProcessUserInput()
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
                if (Input.GetMouseButton(0) && currentlySelectedPlayer != null)
                {
                    var worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                    currentlySelectedPlayer.transform.position = (Vector2)worldPoint;
                    currentlySelectedPlayer.pointsToVisitDuringTraining.Clear();
                    currentlySelectedPlayer.pointsToVisitDuringTraining.Add(currentlySelectedPlayer.transform.position - offsetVector);
                }

                if (Input.GetMouseButtonDown(1) && currentlySelectedPlayer != null)
                {
                    var worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                    currentlySelectedPlayer.pointsToVisitDuringTraining.Add(new Vector3(worldPoint.x, worldPoint.y) - offsetVector);
                }
                if (Input.GetMouseButtonDown(2) && currentlySelectedPlayer != null)
                {
                    currentlySelectedPlayer.pointsToVisitDuringTraining.Clear();
                    currentlySelectedPlayer.pointsToVisitDuringTraining.Add(currentlySelectedPlayer.transform.position - offsetVector);
                }
            }
        }
    }
}
