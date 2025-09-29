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
        gameStatsManager.warFunds.ToString();
        crystalsCounter.text = $"Tributes <sprite=98> {gameStatsManager.captureCrystalsCount}";
        warFundsCounter.text = $"War Funds <sprite=93> {gameStatsManager.warFunds}";
        locationInfoPanel?.SetActive(false);
    }
}
