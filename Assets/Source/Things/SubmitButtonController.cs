using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SubmitButtonController : MonoBehaviour
{
    public bool isSavingTask;
    public InputField inputField;

    // Use this for initialization
    void Start()
    {
        var button = GetComponent<Button>();
        button.onClick.AddListener(TaskOnClick);
    }

    void TaskOnClick()
    {
        if (isSavingTask)
        {
            var txt = inputField.text;
            //inputField.text = "";
            Debug.Log("Saved in " + txt);
            try
            {
                SimController.simInstance.SavePopulation(txt);
            }
            catch (Exception e)
            {
                Debug.Log("Couldn't save: " + e);
            }
        }
        else
        {
            var txt = inputField.text;
            //inputField.text = "";
            Debug.Log("Loaded " + txt);
            try
            {
                SimController.simInstance.LoadPopulation(txt);
            }
            catch (Exception e)
            {
                Debug.Log("Couldn't load: " + e);
            }
        }
    }
}
