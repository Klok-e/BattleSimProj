using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

internal class RunEditorButtonContrl : MonoBehaviour
{
    private AssetBundle myLoadedAssetBundle;
    private string scenePath;

    // Use this for initialization
    private void Start()
    {
        scenePath = "Assets/Resources/TrainScene.unity";

        var button = GetComponent<Button>();
        button.onClick.AddListener(Task);
    }

    private void Task()
    {
        SceneManager.LoadScene(scenePath);
    }
}
