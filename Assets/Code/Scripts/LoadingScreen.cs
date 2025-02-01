using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingScreen : MonoBehaviour
{
    public Image loadingScreenPanel;
    public Canvas loadingScreenCanvas;
    public TextMeshProUGUI nowLoadingText;
    public Image loadingScreenIcon;

    private void OnEnable()
    {
        SceneLoader.OnLoadingScene += ShowLoadingScreenUI;
    }
    private void OnDisable()
    {
        SceneLoader.OnLoadingScene -= ShowLoadingScreenUI;
    }

    private void Start()
    {
        loadingScreenIcon.color = new Color(0, 0, 0, 0);
    }
    void ShowLoadingScreenUI()
    {
        loadingScreenCanvas.enabled = true;
        loadingScreenPanel.color = new Color(1, 1, 1, 1);
        nowLoadingText.fontSize = 60;
        loadingScreenIcon.color = new Color(1, 1, 1, 1);
    }
}
