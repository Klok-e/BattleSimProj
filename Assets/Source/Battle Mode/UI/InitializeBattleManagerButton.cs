using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InitializeBattleManagerButton : MonoBehaviour
{
    GameObject panelThisIn;
    Button button;

    // Use this for initialization
    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(Task);

        panelThisIn = transform.parent.gameObject;
    }

    void Task()
    {
        if (BattleManager.battleManagerInst.loadedNets)
        {
            BattleManager.battleManagerInst.InitializeEverything();
            panelThisIn.SetActive(false);
        }
        else
        {
            Debug.Log("Error: nets not loaded");
        }
    }
}
