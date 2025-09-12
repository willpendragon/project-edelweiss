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
        crystalsCounter.text = $"Crystals   <voffset=10><sprite=0></voffset> {gameStatsManager.captureCrystalsCount}";
        warFundsCounter.text = $"War Funds   <voffset=10><sprite=0></voffset> {gameStatsManager.warFunds}";
        locationInfoPanel?.SetActive(false);
    }
}
