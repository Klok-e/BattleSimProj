using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace SimEditor
{
    public class SubmitButtonController : MonoBehaviour
    {
        public InputField inputField;

        // Use this for initialization
        void Start()
        {
            var button = GetComponent<Button>();
            button.onClick.AddListener(TaskOnClick);
        }

        void TaskOnClick()
        {
            var txt = inputField.text;
            Debug.Log("Saved in " + txt);
            try
            {
                GameManagerController.inputManagerInstance.SaveCurrentPlayer(txt);
            }
            catch (Exception e)
            {
                Debug.Log("Couldn't save: " + e);
            }
        }
    }
}