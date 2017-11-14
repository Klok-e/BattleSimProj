using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BattleMode
{
    public class InitializeBattleManagerButton : MonoBehaviour
    {
        private GameObject panelThisIn;
        private Button button;

        // Use this for initialization
        private void Start()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(Task);

            panelThisIn = transform.parent.gameObject;
        }

        private void Task()
        {
            panelThisIn.SetActive(false);
            StartBattleSettings.singleton.SaveThis();
        }
    }
}