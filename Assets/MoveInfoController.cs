using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MoveInfoController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI moveRangeCounterText;
    [SerializeField] TextMeshProUGUI moveBasedDamageCounterText;
    [SerializeField] TextMeshProUGUI manaCostCounterText;
    [SerializeField] TextMeshProUGUI alignmentText;
    void OnEnable()
    {
        SpellcastingController.OnClickedOnSpellButton += UpdateMoveInfoPanelTexts;
    }
    void OnDisable()
    {
        SpellcastingController.OnClickedOnSpellButton -= UpdateMoveInfoPanelTexts;
    }

    void UpdateMoveInfoPanelTexts(Spell clickedSpell)
    {
        // Retrieve Information from Spell and popoulate the Move Info Panel
    }

}

