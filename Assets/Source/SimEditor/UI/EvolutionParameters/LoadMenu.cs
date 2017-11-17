using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

namespace SimEditor
{
    public class LoadMenu : MonoBehaviour
    {
        [SerializeField] private Canvas canvasMain;
        [SerializeField] private Canvas canvasSaveLoad;

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
                goButton.transform.localScale = Vector3.one;

                Button tempButton = goButton.GetComponent<Button>();
                tempButton.GetComponentInChildren<Text>().text = savesList[i];
                int tempInt = i;

                tempButton.onClick.AddListener(() => LoadSave(tempInt));
            }
        }

        private void LoadSave(int index)
        {
            Debug.Log("Loading save...");
            GameManagerController.inputManagerInstance.LoadPopToSelectedPlayer(savesList[index]);
            canvasMain.gameObject.SetActive(true);
            canvasSaveLoad.gameObject.SetActive(false);
        }
    }
}
