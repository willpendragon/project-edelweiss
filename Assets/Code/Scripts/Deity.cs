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
    public delegate void DeityJudgment();
    public static event DeityJudgment OnDeityJudgment;


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
        if (battleManager.turnCounter >= 3 && battleManager.turnCounter < 5 && player.GetComponent<SinTracker>().redEnmity < 5)
        {
            DeityAttack();
            StartCoroutine("EndDeityTurn");
            //battleManager.PassTurnToPlayer();
        }
        else if (battleManager.turnCounter == 5)
        {
            DeityGridAlteration();
            StartCoroutine("EndDeityTurn");
            //battleManager.PassTurnToPlayer();
        }
        else if (battleManager.turnCounter == 6)
        {
            DeityJudgmentMove();
            StartCoroutine("EndDeityTurn");
            //battleManager.PassTurnToPlayer();
        }
        else if (battleManager.turnCounter < 3)
        {
            {
                deityAttackNotification.text = "The Deity is preparing to unleash its attack";
                StartCoroutine("EndDeityTurn");
                //battleManager.PassTurnToPlayer();
            }
        }
        else if (battleManager.turnCounter >= 3 && player.GetComponent<SinTracker>().redEnmity >= 5)
        {
            if (battleManager.fieldEffectStatus == FieldEffectStatus.inactive)
            {
                DeityFieldEffect();
                StartCoroutine("EndDeityTurn");
                //battleManager.PassTurnToPlayer();
            }
            else
            {
                DeityAttack();
                StartCoroutine("EndDeityTurn");
                //battleManager.PassTurnToPlayer();
            }
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
        int selectedTileIndex = Random.Range(1, tileNumber);
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

