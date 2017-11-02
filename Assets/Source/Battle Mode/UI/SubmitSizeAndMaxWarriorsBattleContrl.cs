using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SubmitSizeAndMaxWarriorsBattleContrl : MonoBehaviour
{
    [SerializeField] GameObject mainCanvas;
    [SerializeField] GameObject thisCanvas;

    [SerializeField] InputField width;
    [SerializeField] InputField height;
    [SerializeField] InputField maxWarr;

    Button button;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(Task);
    }

    void Task()
    {
        int hei;
        bool heiSuc = int.TryParse(height.text, out hei);
        int wid;
        bool widSuc = int.TryParse(width.text, out wid);
        int maxW;
        bool maxWSuc = int.TryParse(maxWarr.text, out maxW);

        if (heiSuc && widSuc && maxWSuc)
        {
            mainCanvas.SetActive(true);
            thisCanvas.SetActive(false);
            BattleManager.battleManagerInst.SetMapSizeAndWarriors(wid, hei, maxW);
        }
        else
        {
            Debug.Log("Error");
        }
    }
}
