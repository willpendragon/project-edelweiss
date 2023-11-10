using UnityEngine;
using UnityEngine.UI;

public class SpellMenu : MonoBehaviour
{
    public Spell[] spells; // Array of Spell ScriptableObjects
    public GameObject spellButtonPrefab;
    public Transform buttonContainer;
    public float verticalSpacing = 10f; // Vertical spacing between buttons

    void Start()
    {
        PopulateSpellMenu();
    }
    void PopulateSpellMenu()
    {
        float buttonHeight = spellButtonPrefab.GetComponent<RectTransform>().rect.height;

        foreach (Spell spell in spells)
        {
            GameObject spellButton = Instantiate(spellButtonPrefab, buttonContainer);
            spellButton.GetComponentInChildren<Text>().text = spell.spellName;

            // Calculate the vertical position for the current button
            float yPos = -buttonContainer.childCount * (buttonHeight + verticalSpacing);

            // Set the vertical position of the button
            RectTransform buttonRectTransform = spellButton.GetComponent<RectTransform>();
            buttonRectTransform.anchoredPosition = new Vector2(buttonRectTransform.anchoredPosition.x, yPos);

            // Add a listener to the button's click event and pass the corresponding spell
            spellButton.GetComponent<Button>().onClick.AddListener(() => CastSpell(spell));
        }
    }
    void CastSpell(Spell spell)
    {
        // Implement spell casting logic here
        Debug.Log("Casting " + spell.spellName + " with damage: " + spell.damage + " and mana cost: " + spell.manaCost);
        // Execute your spell logic, e.g., dealing damage to enemies, consuming mana, etc.
    }
}