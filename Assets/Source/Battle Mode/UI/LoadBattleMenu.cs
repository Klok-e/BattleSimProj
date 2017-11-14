using SharpNeat.Phenomes;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BattleMode
{
    public class LoadBattleMenu : MonoBehaviour
    {
        [SerializeField] private GameObject toEnableAfterPress;
        [SerializeField] private GameObject thisAt;

        [SerializeField] private Transform menuPanel;
        [SerializeField] private GameObject buttonPrefab;
        private List<string> savesList;

        public void Refresh()
        {
            foreach (Transform item in menuPanel.transform)
            {
                Destroy(item.gameObject);
            }

            savesList = SaveLoad.savedGames;
            for (int i = 0; i < savesList.Count; i++)
            {
                GameObject goButton = (GameObject)Instantiate(buttonPrefab);
                goButton.transform.SetParent(menuPanel, false);
                goButton.transform.localScale = new Vector3(1, 1, 1);

                Button tempButton = goButton.GetComponent<Button>();
                tempButton.GetComponentInChildren<Text>().text = savesList[i];
                int tempInt = i;

                tempButton.onClick.AddListener(() => LoadSavePathToStartBattleSettings(tempInt));
            }
        }

        private void LoadSavePathToStartBattleSettings(int index)
        {
            Debug.Log("Loading save...");
            StartBattleSettings.singleton.AddNets(savesList[index]);
            toEnableAfterPress.gameObject.SetActive(true);
            thisAt.gameObject.SetActive(false);
        }
    }
}
