using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class CreateNewMapButtonController : MonoBehaviour
{
    public InputField inputHeight;
    public InputField inputWidth;

    // Use this for initialization
    void Start()
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(TaskOnClick);
    }

    void TaskOnClick()
    {
        int width;
        int height;
        try
        {
            int.TryParse(inputWidth.text, out width);
            int.TryParse(inputHeight.text, out height);

            GameManagerController.inputManagerInstance.simInst.CreateNewMap(width, height);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }
}
