using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ExitButtonScript : MonoBehaviour
{
    private Button button;

    // Use this for initialization
    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(Task);
    }

    private void Task()
    {
        Application.Quit();
    }
}
