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
        if (state == false && !stopping)//if initial state is start algorithm
        {
            Debug.Log("Starting...");
            state = true;
            textObj.text = "Stop";
            GameManagerController.inputManagerInstance.StartRunningGenerations();
        }
        else if (state == true && !stopping)
        {
            Debug.Log("Stopping...");
            state = false;
            GameManagerController.inputManagerInstance.StopRunningGenerations();
            StartCoroutine(WaitTillStopped());
        }
    }

    bool stopping;
    IEnumerator WaitTillStopped()
    {
        stopping = true;
        while (true)
        {
            if (!GameManagerController.inputManagerInstance.simInst.mutualEvaluatingFlag)
            {
                stopping = false;
                textObj.text = "Start";
                break;
            }
            textObj.text = "Stopping...";
            yield return null;
        }
    }
}
