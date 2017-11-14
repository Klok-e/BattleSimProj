using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

internal class RunBattleModeBttnCntrl : MonoBehaviour
{
    private AssetBundle myLoadedAssetBundle;
    private string scenePath;

    // Use this for initialization
    private void Start()
    {
        scenePath = "Assets/Resources/BattleScene.unity";

        var button = GetComponent<Button>();
        button.onClick.AddListener(Task);
    }

    private void Task()
    {
        SceneManager.LoadScene(scenePath);
    }
}
