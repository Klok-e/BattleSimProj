using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class LoadBattleMenu : MonoBehaviour
{
    [SerializeField] Canvas canvasMain;
    [SerializeField] Canvas canvasSaveLoad;

    [SerializeField] Transform menuPanel;
    [SerializeField] GameObject buttonPrefab;
    List<string> savesList;

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

            tempButton.onClick.AddListener(() => LoadSave(tempInt));
        }
    }
    void LoadSave(int index)
    {
        Debug.Log("Loading save...");
        GameManagerController.inputManagerInstance.LoadPopToSelectedPlayer(savesList[index]);
        canvasMain.gameObject.SetActive(true);
        canvasSaveLoad.gameObject.SetActive(false);
    }
}