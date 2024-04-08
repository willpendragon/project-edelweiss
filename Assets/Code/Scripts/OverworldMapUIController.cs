using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class OverworldMapUIController : MonoBehaviour
{
    public TextMeshProUGUI warFundsCounter;
    public GameStatsManager gameStatsManager;
    void Start()
    {
        warFundsCounter.text = gameStatsManager.warFunds.ToString();
    }
}
