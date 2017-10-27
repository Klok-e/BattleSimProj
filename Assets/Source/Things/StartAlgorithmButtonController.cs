using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartAlgorithmButtonController : MonoBehaviour
{
    bool state;
    Button button;
    Text textObj;

    // Use this for initialization
    void Start()
    {
        button = GetComponent<Button>();
        textObj = button.GetComponentInChildren<Text>();
        button.onClick.AddListener(Task);
    }
    void Task()
    {
        if (state == false)//if initial state is start algorithm
        {
            Debug.Log("Starting...");
            state = true;
            textObj.text = "Stop";
            GameManagerController.inputManagerInstance.StartRunningGenerations();
        }
        else
        {
            Debug.Log("Stopping...");
            state = false;
            textObj.text = "Start";
            GameManagerController.inputManagerInstance.StopRunningGenerations();
        }
    }
}
