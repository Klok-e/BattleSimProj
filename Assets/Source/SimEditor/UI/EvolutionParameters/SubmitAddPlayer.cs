using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SimEditor
{
    public class SubmitAddPlayer : MonoBehaviour
    {
        [SerializeField] private InputField warriorsForNewPlayer;
        [SerializeField] private Toggle toggleFreezeTraining;

        // Use this for initialization
        private void Start()
        {
            var button = GetComponent<Button>();
            button.onClick.AddListener(TaskOnClick);
        }

        private void TaskOnClick()
        {
            bool frz = toggleFreezeTraining.isOn;

            int warrs;
            bool succ = int.TryParse(warriorsForNewPlayer.text, out warrs);

            if (succ)
            {
                GameManagerController.inputManagerInstance.AddPlayer(warrs, frz);
            }
            else
            {
                Debug.Log("Could not parse text");
            }
        }
    }
}
