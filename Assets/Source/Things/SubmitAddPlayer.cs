using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SubmitAddPlayer : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
        var button = GetComponent<Button>();
        button.onClick.AddListener(TaskOnClick);
    }

    void TaskOnClick()
    {
        //var txt = inpField.text;

        //int num;
        //bool succeded = int.TryParse(txt, out num);

        GameManagerController.inputManagerInstance.AddPlayer(HelperConstants.warriorsPerPlayer);
    }
}
