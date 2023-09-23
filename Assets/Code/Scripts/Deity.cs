using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public enum alignmentType
{
    red,
    blue
}

public enum blackListedAlignment
{
    red,
    blue
}

public enum DeityState
{
    indifferent,
    aggressive,
    gridaltering
}

public class Deity : MonoBehaviour
{

    public blackListedAlignment currentBlacklistedAlignment;
    public alignmentType deityAlignment;
    public List<GameObject> sinnersList;
    public Player player;
    public TextMeshProUGUI deityAttackNotification;
    public BattleManager battleManager;
    public BattleInterface battleInterface;
    [SerializeField] float enmity;
    [SerializeField] GameObject deityAttackVFX;
    [SerializeField] GameObject deityFieldEffectVFX;
    public int judgmentTurnLimit;
    public DeityState currentDeityState;

    public delegate void DeityJudgment();
    public static event DeityJudgment OnDeityJudgment;

    public delegate void DeityJudgmentCounterUpdate(int judgmentTurnLimitNumber);
    public static event DeityJudgmentCounterUpdate OnDeityJudgmentCounterUpdate;
    private void OnEnable()
    {
        EnemyTurnManager.OnDeityTurn += DeityBehaviour;
    }
    private void OnDisable()
    {
        EnemyTurnManager.OnDeityTurn -= DeityBehaviour;
    }

    public void Start()
    {
        OnDeityJudgmentCounterUpdate(judgmentTurnLimit);
    }
    public void SinTracker(attackAlignmentType currentAttackAlignmentType, GameObject unit)
    //This function keeps track of the behaviour of the Player during the gameplay and increases the Enmity
    //value in relation to the type of the Move chosen by the Player.
    {
        if ((int)currentAttackAlignmentType == (int)currentBlacklistedAlignment)
        {
            if (sinnersList.Contains(unit) != true)
            {
                sinnersList.Add(unit);
                //The Unit who uses a Move of the opposite alignment of the Deity is added to
                //a Sinner's list. Being in the Sinner's list means that the Deity subsequent attack
                //will sport additional effects and modifiers, based on the accrued Enmity. 
                Debug.Log("Unit added to Sinners List");
            }
            unit.GetComponent<SinTracker>().IncreaseEnmity(deityAlignment);
        }
    }

    public void DeityBehaviour()
    //A simple method that informs the Deity logic. Needs refactoring after prototyping.
    //Consider changing the current implementation with a system based on Switch/Case.
    {
        StartCoroutine("EndDeityTurn");
        Debug.Log("Deity Behaviour");
        //At Turn 1 & 2, this Deity stays Indifferent and just observes the battlefield.
        if (battleManager.turnCounter == 1 || battleManager.turnCounter == 2)
        {
            currentDeityState = DeityState.indifferent;
        }
        //At Turn 3, this Deity casts a Deity Field Effect on the battlefield.
        else if (battleManager.turnCounter == 3)
        {
            DeityFieldEffect();
        }
        //At Turn 4 & 5, this Deity casts a powerful attack (Blizzard).
        else if (battleManager.turnCounter == 4 || battleManager.turnCounter == 5)
        {
            DeityAttack();
        }
        //From Turn 6 until Turn 8, the Deity controls the Battleground grids and change the tiles alignment to Blue.
        else if (battleManager.turnCounter <= 8)
        {
            DeityGridAlteration();
        }
        //At Turn 9, the Deity unleashes the Judgment move. If the Player survives, they won the battle.
        else if (battleManager.turnCounter == judgmentTurnLimit)
        {
            DeityJudgmentMove();
            Debug.Log("Deity Judgment attack");
        }
    }
    public void DeityAttack()
    //A simple attack from the Deity that deals damage to the Player after checking it's accrued enmity.
    {
        if (deityAttackVFX != null)
        {
            deityAttackVFX.SetActive(true);
        }
        player.healthPoints -= 10 * player.GetComponent<SinTracker>().redEnmity;
        player.UpdatePlayerHealthDisplay();
        deityAttackNotification.text = "The Deity has Attacked";
        StartCoroutine("ResetDeityAttack");
    }

    public void DeityFieldEffect()
    //A simple effect provoked by the Deity that affects the Units on the Battlefield with varying effects.
    {
        if (deityFieldEffectVFX != null)
        {
            deityFieldEffectVFX.SetActive(true);
        }
        player.currentFieldEffect = fieldEffect.iceMist;
        player.UpdatePlayerHealthDisplay();
        player.PlayHurtAnimation();
        deityAttackNotification.text = "The Deity has created a Field Effect";
        battleManager.fieldEffectStatus = FieldEffectStatus.active;
        StartCoroutine("ResetDeityAttack");
    }
    public void DeityGridAlteration()
    //The Deity performs this Move as the end of the battle approaches. The Deity changes the property of a Tile,
    //transforming it into a "Red" or "Blue" spot.
    {
        TileController[] tileControllers = FindObjectsOfType<TileController>();
        int tileNumber = tileControllers.Length;
        int selectedTileIndex = Random.Range(0, tileNumber);
        TileController selectedTileController = tileControllers[selectedTileIndex].GetComponent<TileController>();

        if (selectedTileController.currentTileAlignment == TileController.TileAlignment.neutral)
        {
            selectedTileController.ActivateBlueParticle();
        }
    }
    public void DeityJudgmentMove()
    {
        OnDeityJudgment();
    }
    IEnumerator ResetDeityAttack()
    //This coroutine triggers at the end of the Deity's attack, resetting the UI displays and the VFX.
    {
        yield return new WaitForSeconds(1);
        if (deityAttackVFX != null && deityAttackVFX == isActiveAndEnabled)
        {
            deityAttackVFX.SetActive(false);
        }
        deityAttackNotification.text = "";
        //battleManager.turnCounter = 0;
        //battleManager.turnTracker.text = "0";
    }

    IEnumerator EndDeityTurn()
    {   
        yield return new WaitForSeconds(1f);
        battleManager.PassTurnToPlayer();
    }
}

