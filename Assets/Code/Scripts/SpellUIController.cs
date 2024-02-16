using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpellUIController : MonoBehaviour
{
    public SpellcastingController spellCastingController;
    public GameObject spellButtonPrefab;
    public Transform spellMenuContainer;

    public void PopulateCharacterSpellsMenu(GameObject detectedUnit)
    {
        List<Spell> spells = GetCharacterSpells(detectedUnit);
        //Generates Buttons linked to each Spell.
        foreach (Spell spell in spells)
        {
            GameObject spellButtonInstance = Instantiate(spellButtonPrefab, spellMenuContainer);
            Button currentSpellButton = spellButtonInstance.GetComponent<Button>();
            currentSpellButton.GetComponentInChildren<Text>().text = spell.spellName;
            if (spell.spellType == SpellType.aoe)
            {
                //currentSpellButton.onClick.AddListener(() => spellCastingController.CastSpell(spell));
                currentSpellButton.onClick.AddListener(() => SwitchTilesToSpellMode());
                currentSpellButton.onClick.AddListener(() => spellCastingController.SetCurrentSpell(spell));
            }
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
    public List<Spell> GetCharacterSpells(GameObject detectedUnit)
    {
        //GameObject activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit");
        List<Spell> activePlayerUnitSpellsList = detectedUnit.GetComponent<Unit>().unitTemplate.spellsList;
        return activePlayerUnitSpellsList;
    }

    public void SwitchTilesToSpellMode()
    {
        //After clicking the Melee Button, all of the Grid Map tiles switch to Selection Mode
        foreach (var tile in GridManager.Instance.gridTileControllers)
        {
            tile.currentPlayerAction = new AOESpellPlayerAction();
            tile.currentSingleTileStatus = SingleTileStatus.selectionMode;
            Debug.Log("Switching tiles to AOE Spell Mode");
        }

    }
}
