using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class OverworldMapUIController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI warFundsCounter;
    [SerializeField] TextMeshProUGUI crystalsCounter;
    [SerializeField] GameObject locationInfoPanel;
    public GameStatsManager gameStatsManager;
    void Start()
    {
        warFundsCounter.text = gameStatsManager.warFunds.ToString();
        crystalsCounter.text = gameStatsManager.captureCrystalsCount.ToString();
        locationInfoPanel?.SetActive(false);
    }
}
