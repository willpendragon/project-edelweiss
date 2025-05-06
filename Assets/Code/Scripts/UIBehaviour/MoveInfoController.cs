using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MoveInfoController : MonoBehaviour
{
    public static MoveInfoController Instance { get; private set; }

    private enum PlayerAction
    {
        Move,
        Melee,
        Spell,
        Trap,
        Magnet,
        Summon,
        Capture
    }

    [SerializeField] TextMeshProUGUI moveRangeCounterText;
    [SerializeField] TextMeshProUGUI moveBaseDamageCounterText;
    [SerializeField] TextMeshProUGUI manaCostCounterText;
    [SerializeField] TextMeshProUGUI alignmentText;
    [SerializeField] TextMeshProUGUI helpPanelText;

    [SerializeField] RectTransform helpPanel;
    [SerializeField] Transform moveInfoPanel;

    private PlayerAction currentPlayerAction;
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
        SpellcastingController.OnClickedOnSpellButton += UpdateActionInfoPanelTexts;
        AOESpellPlayerAction.OnSelectedSpell += DisplayActionInfoPanel;
        AOESpellPlayerAction.OnDeselectedSpell += HideActionInfoPanel;
        MakeActivePlayerUnitWait.OnPlayerWaiting += HideActionInfoPanel;
        BattleInterface.OnDeselectPlayerAction += HideActionInfoPanel;
    }
    void OnDisable()
    {
        SpellcastingController.OnClickedOnSpellButton -= UpdateActionInfoPanelTexts;
        AOESpellPlayerAction.OnSelectedSpell -= DisplayActionInfoPanel;
        AOESpellPlayerAction.OnDeselectedSpell -= HideActionInfoPanel;
        MakeActivePlayerUnitWait.OnPlayerWaiting -= HideActionInfoPanel;
        BattleInterface.OnDeselectPlayerAction -= HideActionInfoPanel;
    }
    private void Start()
    {
        HideActionInfoPanel();
        HideHelpPanel();
    }
    private void UpdateActionInfoPanelTexts(Spell clickedSpell)
    {
        // Retrieve Information from Spell and popoulate the Move Info Panel.
        DisplayActionInfoPanel();
        currentPlayerAction = PlayerAction.Spell;

        moveRangeCounterText.text = clickedSpell.spellRange.ToString();
        moveBaseDamageCounterText.text = clickedSpell.damage.ToString();
        manaCostCounterText.text = clickedSpell.manaPointsCost.ToString();
        alignmentText.text = clickedSpell.alignment.ToString();
    }
    public void UpdateMeleeMoveInfoPanelTexts()
    {
        // Retrieve Information from Melee Move and popoulate the Move Info Panel.
        DisplayActionInfoPanel();

        int meleeRange = 2;

        // Hard-coded logic, I actually need to pass this value from the Melee Action itself.
        moveRangeCounterText.text = meleeRange.ToString();
        Unit currentActiveUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();
        manaCostCounterText.text = "-";
        alignmentText.text = "-";

        if (currentActiveUnit.hasHookshot == false)
        {
            currentPlayerAction = PlayerAction.Melee;
            moveBaseDamageCounterText.text = currentActiveUnit.unitMeleeAttackBaseDamage.ToString();
        }
        else if (currentActiveUnit.hasHookshot == true)
        {
            currentPlayerAction = PlayerAction.Magnet;
            moveBaseDamageCounterText.text = "-";
        }
    }
    public void UpdateTrapMoveInfoPanelTexts()
    {
        // Retrieve Information from Trap and popoulate the Move Info Panel
        DisplayActionInfoPanel();
        currentPlayerAction = PlayerAction.Trap;

        int trapCreationRange = 1;

        // Hard-coded logic, I actually need to pass this value from the Trap Action itself.
        moveRangeCounterText.text = trapCreationRange.ToString();
        Unit currentActiveUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();
        manaCostCounterText.text = "-";
        alignmentText.text = "-";
        moveBaseDamageCounterText.text = "-";
    }
    public void DisplayActionInfoPanel()
    {
        moveInfoPanel.localScale = Vector3.one;
    }
    public void HideActionInfoPanel()
    {
        moveInfoPanel.localScale = Vector3.zero;
    }
    public void UpdateHelpPanel()
    {
        helpPanel.localScale = Vector3.one;
        switch (currentPlayerAction)
        {
            case PlayerAction.Melee:
                helpPanelText.text = "A physical blow. May attack the enemy even from a distance.";
                break;
            case PlayerAction.Magnet:
                helpPanelText.text = "A Magitechnica artifact. Allows to attract an enemy on a tile adjacent to the user.";
                break;
            case PlayerAction.Spell:
                helpPanelText.text = "An ethereal blow. May have different ranges. Its alignment could upset the Deity.";
                break;
            case PlayerAction.Trap:
                helpPanelText.text = "Send an Enemy over a trap to eliminate it instantly during its turn and gain Capture Crystals.";
                break;
        }
    }
    public void HideHelpPanel()
    {
        helpPanel.localScale = Vector3.zero;
    }
}
