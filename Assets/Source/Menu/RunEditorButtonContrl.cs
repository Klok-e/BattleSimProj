using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RunEditorButtonContrl : MonoBehaviour
{
    AssetBundle myLoadedAssetBundle;
    string scenePath;
    // Use this for initialization
    void Start()
    {
        scenePath = "Assets/Resources/TrainScene.unity";

        var button = GetComponent<Button>();
        button.onClick.AddListener(Task);
    }

    void Task()
    {
        SceneManager.LoadScene(scenePath);
    }
}
