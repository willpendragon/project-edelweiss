using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CapsuleCrystalCounterHandler : MonoBehaviour
{
    public Text capsuleCounterText;

    private void Start()
    {
        UpdateCapsuleCounterText();
    }
    public void UpdateCapsuleCounterText()
    {
        GameStatsManager gameStatsManager = GameObject.FindGameObjectWithTag("GameStatsManager").GetComponent<GameStatsManager>();
        capsuleCounterText.text = gameStatsManager.captureCrystalsCount.ToString();
    }
}
