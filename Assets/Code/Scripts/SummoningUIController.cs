using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SummoningUIController : MonoBehaviour
{
    public SummoningController summoningCastingController;
    public GameObject summonButtonPrefab;
    public Transform spellMenuContainer;

    public void Start()
    {
        summoningCastingController = GameObject.FindGameObjectWithTag("SummoningController").GetComponent<SummoningController>();
    }
    public void AddSummonButton()
    //I will need to create a Pokémon-Style menu to summon the collected deities.
    {
        GameObject summonButtonInstance = Instantiate(summonButtonPrefab, spellMenuContainer);
        Button currentSummonButton = summonButtonInstance.GetComponent<Button>();
        currentSummonButton.GetComponentInChildren<Text>().text = "Summon";
        currentSummonButton.onClick.AddListener(() => summoningCastingController.StartSummoningRitual());
    }
}
