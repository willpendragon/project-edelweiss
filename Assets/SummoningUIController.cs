using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SummoningUIController : MonoBehaviour
{
    public SummoningController summoningCastingController;
    public GameObject summonButtonPrefab;
    public Transform spellMenuContainer;

    // Start is called before the first frame update
    public void AddSummonButton()
    //I will need to create a Pokémon-Style menu to summon the collected deities.
    {
        GameObject spellButtonInstance = Instantiate(summonButtonPrefab, spellMenuContainer);
        Button currentSummonButton = spellButtonInstance.GetComponent<Button>();
        currentSummonButton.GetComponentInChildren<Text>().text = "Summon";
        currentSummonButton.onClick.AddListener(() => summoningCastingController.SummonDeityOnBattlefield());
    }
}
