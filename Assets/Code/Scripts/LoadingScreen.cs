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
    private void OnEnable()
    {
        CheckRequirement.OnLoadingBattle += ShowLoadingScreenUI;
    }
    private void OnDisable()
    {
        CheckRequirement.OnLoadingBattle -= ShowLoadingScreenUI;
    }
    void ShowLoadingScreenUI()
    {
        loadingScreenPanel.color = new Color(1,1,1,1);
        nowLoadingText.fontSize = 60;
        loadingScreenCanvas.enabled = true;
    }
}
