using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace SimEditor
{
    public class SubmitAddPlayer : MonoBehaviour
    {
        public Toggle toggleFreezeTraining;
        // Use this for initialization
        void Start()
        {
            var button = GetComponent<Button>();
            button.onClick.AddListener(TaskOnClick);
        }

        void TaskOnClick()
        {
            bool frz = toggleFreezeTraining.isOn;

            GameManagerController.inputManagerInstance.AddPlayer(HelperConstants.warriorsPerPlayer, frz);
        }
    }
}