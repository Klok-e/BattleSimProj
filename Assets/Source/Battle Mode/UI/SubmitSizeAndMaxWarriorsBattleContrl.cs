using UnityEngine;
using UnityEngine.UI;

namespace BattleMode
{
    public class SubmitSizeAndMaxWarriorsBattleContrl : MonoBehaviour
    {
        [SerializeField] private GameObject panelToEnableAfterPress;
        [SerializeField] private GameObject thisPanel;
        [SerializeField] private LoadBattleMenu loadGenomesMenu;

        [SerializeField] private InputField width;
        [SerializeField] private InputField height;
        [SerializeField] private InputField maxWarr;

        private Button button;

        private void Start()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(Task);
        }

        private void Task()
        {
            int hei;
            bool heiSuc = int.TryParse(height.text, out hei);
            int wid;
            bool widSuc = int.TryParse(width.text, out wid);
            int maxW;
            bool maxWSuc = int.TryParse(maxWarr.text, out maxW);

            if (heiSuc && widSuc && maxWSuc)
            {
                panelToEnableAfterPress.SetActive(true);
                thisPanel.SetActive(false);
                new StartBattleSettings();
                StartBattleSettings.singleton.SetSize(wid, hei);
                StartBattleSettings.singleton.SetWarriors(maxW);
                StartBattleSettings.singleton.ResetNets();
                SaveLoad.Load();
                loadGenomesMenu.Refresh();
            }
            else
            {
                Debug.Log("Error");
            }
        }
    }
}