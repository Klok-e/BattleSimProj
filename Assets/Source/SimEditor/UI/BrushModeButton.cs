using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SimEditor
{
    public class BrushModeButton : MonoBehaviour
    {
        private Button button;

        private bool state;

        private string initText;

        // Use this for initialization
        private void Start()
        {
            button = GetComponent<Button>();

            initText = button.GetComponentInChildren<Text>().text;

            button.onClick.AddListener(Task);
        }

        private void Task()
        {
            if (state == false)
            {
                button.GetComponentInChildren<Text>().text = "Stop changing";
                state = true;

                GameManagerController.inputManagerInstance.SetBrushMode(true);
            }
            else
            {
                button.GetComponentInChildren<Text>().text = initText;
                state = false;

                GameManagerController.inputManagerInstance.SetBrushMode(false);
            }
        }
    }
}
