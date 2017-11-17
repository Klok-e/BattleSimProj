using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SimEditor
{
    public class StartAlgorithmButtonController : MonoBehaviour
    {
        private bool state;
        private Button button;
        private Text textObj;
        [SerializeField] private Button toDisableWhenStarted;

        // Use this for initialization
        private void Start()
        {
            button = GetComponent<Button>();
            textObj = button.GetComponentInChildren<Text>();
            button.onClick.AddListener(Task);
        }

        private void Task()
        {
            if (state == false && !stopping)//if initial state is start algorithm
            {
                if (GameManagerController.inputManagerInstance.AnyPlayersPresent())
                {
                    Debug.Log("Starting...");
                    state = true;
                    textObj.text = "Stop";
                    toDisableWhenStarted.gameObject.SetActive(false);
                    GameManagerController.inputManagerInstance.StartRunningGenerations();
                }
            }
            else if (state == true && !stopping)
            {
                Debug.Log("Stopping...");
                state = false;
                GameManagerController.inputManagerInstance.StopRunningGenerations();
                StartCoroutine(WaitTillStopped());
            }
        }

        private bool stopping;

        private IEnumerator WaitTillStopped()
        {
            stopping = true;
            while (true)
            {
                if (!GameManagerController.inputManagerInstance.simInst.mutualEvaluatingFlag)
                {
                    stopping = false;
                    textObj.text = "Start";
                    toDisableWhenStarted.gameObject.SetActive(true);
                    break;
                }
                textObj.text = "Stopping...";
                yield return null;
            }
        }
    }
}
