using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpellUIController : MonoBehaviour
{
    public SpellcastingController spellCastingController;
    public GameObject spellButtonPrefab;
    public Transform spellMenuContainer;
    /*
    public void OnEnable()
    {
        UnitSelectionController.OnActiveCharacterSelected += PopulateCharacterSpellsMenu;
    }
    public void OnDisable()
    {
        UnitSelectionController.OnActiveCharacterSelected += PopulateCharacterSpellsMenu;
    }
    */
    public void PopulateCharacterSpellsMenu()
    {
        List<Spell> spells = GetCharacterSpells();
        //Generates Buttons linked to each Spell.
        foreach (Spell spell in spells)
        {
            GameObject spellButtonInstance = Instantiate(spellButtonPrefab, spellMenuContainer);
            Button currentSpellButton = spellButtonInstance.GetComponent<Button>();
            currentSpellButton.GetComponentInChildren<Text>().text = spell.spellName;
            currentSpellButton.onClick.AddListener(() => spellCastingController.CastSpell(spell));
        }

    }
    public void ResetCharacterSpellsMenu()
    {
        GameObject[] playerUISpellButtons = GameObject.FindGameObjectsWithTag("PlayerUISpellButton");
        foreach (var playerUISpellButton in playerUISpellButtons)
        {
            Destroy(playerUISpellButton);
        }
    }

    //Retrieves a list of Spell from the Active Character.
    public List<Spell> GetCharacterSpells()
    {
        GameObject activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit");
        List<Spell> activePlayerUnitSpellsList = activePlayerUnit.GetComponent<Unit>().unitTemplate.spellsList;
        return activePlayerUnitSpellsList;
    }
}
