using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SpellUIController : MonoBehaviour
{
    public void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public SpellcastingController spellCastingController;
    public GameObject spellButtonPrefab;
    public Transform spellMenuContainer;

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "battle_prototype" || scene.name == "boss_battle_prototype")
        {
            spellCastingController = GameObject.FindGameObjectWithTag("SpellcastingController").GetComponent<SpellcastingController>();
            spellMenuContainer = GameObject.FindGameObjectWithTag("MovesPanel").transform;
        }
    }

    public void PopulateCharacterSpellsMenu(GameObject detectedUnit)
    {
        List<Spell> spells = GetCharacterSpells(detectedUnit);
        //Generates Buttons linked to each Spell.
        foreach (Spell spell in spells)
        {
            GameObject spellButtonInstance = Instantiate(spellButtonPrefab, spellMenuContainer);
            Button currentSpellButton = spellButtonInstance.GetComponent<Button>();
            currentSpellButton.GetComponentInChildren<Text>().text = spell.spellName;
            if (spell.spellType == SpellType.AOE)
            {
                //currentSpellButton.onClick.AddListener(() => spellCastingController.CastSpell(spell));
                currentSpellButton.onClick.AddListener(() => SwitchTilesToSpellMode());
                currentSpellButton.onClick.AddListener(() => spellCastingController.SetCurrentSpell(spell));
            }
            else if (spell.spellType == SpellType.SingleTarget)
            {
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
        List<Spell> activePlayerUnitSpellsList = detectedUnit.GetComponent<Unit>().unitTemplate.spellsList;
        return activePlayerUnitSpellsList;
    }

    public void SwitchTilesToSpellMode()
    {
        DestroyMagnet();
        DeactivateTrapSelection();

        //After clicking the Spell Button, all of the Grid Map tiles switch to Selection Mode
        foreach (var tile in GridManager.Instance.gridTileControllers)
        {
            tile.currentPlayerAction = new AOESpellPlayerAction();
            tile.currentSingleTileStatus = SingleTileStatus.selectionMode;
            tile.gameObject.GetComponentInChildren<TileShaderController>().AnimateFadeHeight(0, 0.2f, Color.white);
        }
    }
    void DestroyMagnet()
    {
        Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();
        if (activePlayerUnit != null && activePlayerUnit.hasHookshot == true)
        {
            MagnetHelper magnetHelper = activePlayerUnit.gameObject.GetComponentInChildren<MagnetHelper>();
            magnetHelper.DestroyMagnet();
        }
    }
    public void DeactivateTrapSelection()
    {
        GridManager.Instance.RemoveTrapSelection();
    }
}
