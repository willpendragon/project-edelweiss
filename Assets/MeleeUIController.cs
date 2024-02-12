using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MeleeUIController : MonoBehaviour

{
    public MeleeController meleeController;
    public GameObject meleeButtonPrefab;
    public Transform spellMenuContainer;
    private string buttonName;

    public void AddMeleeButton()
    {
        //Instantiates Melee Button.
        GameObject meleeButtonInstance = Instantiate(meleeButtonPrefab, spellMenuContainer);
        Button currentMeleeButton = meleeButtonInstance.GetComponent<Button>();
        //currentSpellButton.GetComponentInChildren<Text>().text = buttonName;
        currentMeleeButton.onClick.AddListener(() => meleeController.StartMeleeAttack());
    }
}