using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MoveInfoController : MonoBehaviour
{
    public static MoveInfoController Instance { get; private set; }

    [SerializeField] TextMeshProUGUI moveRangeCounterText;
    [SerializeField] TextMeshProUGUI moveBaseDamageCounterText;
    [SerializeField] TextMeshProUGUI manaCostCounterText;
    [SerializeField] TextMeshProUGUI alignmentText;
    [SerializeField] Transform moveInfoPanel;
    void Awake()
    {
        // Ensure only one instance exists
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void OnEnable()
    {
        SpellcastingController.OnClickedOnSpellButton += UpdateMoveInfoPanelTexts;
        AOESpellPlayerAction.OnSelectedSpell += DisplayMoveInfoPanel;
        AOESpellPlayerAction.OnDeselectedSpell += HideMoveInfoPanel;
        MakeActivePlayerUnitWait.OnPlayerWaiting += HideMoveInfoPanel;
    }
    void OnDisable()
    {
        SpellcastingController.OnClickedOnSpellButton -= UpdateMoveInfoPanelTexts;
        AOESpellPlayerAction.OnSelectedSpell -= DisplayMoveInfoPanel;
        AOESpellPlayerAction.OnDeselectedSpell -= HideMoveInfoPanel;
        MakeActivePlayerUnitWait.OnPlayerWaiting -= HideMoveInfoPanel;
    }

    private void Start()
    {
        HideMoveInfoPanel();
    }

    public void UpdateMoveInfoPanelTexts(Spell clickedSpell)
    {
        // Retrieve Information from Spell and popoulate the Move Info Panel
        DisplayMoveInfoPanel();

        moveRangeCounterText.text = clickedSpell.spellRange.ToString();
        moveBaseDamageCounterText.text = clickedSpell.damage.ToString();
        manaCostCounterText.text = clickedSpell.manaPointsCost.ToString();
        alignmentText.text = clickedSpell.alignment.ToString();
    }

    public void UpdateMeleeMoveInfoPanelTexts()
    {
        // Retrieve Information from Spell and popoulate the Move Info Panel
        DisplayMoveInfoPanel();

        int meleeRange = 3;

        // Hard-coded logic, I actually need to pass this value from the Melee Action itself.
        moveRangeCounterText.text = meleeRange.ToString();
        Unit currentActiveUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();
        manaCostCounterText.text = "-";
        alignmentText.text = "-";

        if (currentActiveUnit.hasHookshot == false)
        {
            moveBaseDamageCounterText.text = currentActiveUnit.unitMeleeAttackBaseDamage.ToString();
        }
        else if (currentActiveUnit.hasHookshot == true)
        {
            moveBaseDamageCounterText.text = "-";
        }
    }


    public void DisplayMoveInfoPanel()
    {
        moveInfoPanel.localScale = new Vector3(1, 1, 1);
    }
    public void HideMoveInfoPanel()
    {
        moveInfoPanel.localScale = new Vector3(0, 0, 0);
    }
}

