using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using static Deity;

public enum DeityState
{
    indifferent,
    aggressive,
    gridaltering
}

public class Deity : MonoBehaviour
{

    //public blackListedAlignment currentBlacklistedAlignment;
    //public alignmentType deityAlignment;
    public List<GameObject> sinnersList;
    //public Player player;
    public TextMeshProUGUI deityAttackNotification;
    public BattleManager battleManager;
    public BattleInterface battleInterface;
    public float enmity;
    public GameObject deityAttackVFX;
    //[SerializeField] GameObject deityFieldEffectVFX;
    //public int judgmentTurnLimit;
    public DeityState currentDeityState;
    //public int judgmentAttackPower = 1000;
    public List<SpellAlignment> hatedSpellAlignments;
    public GameObject deityEnmityTracker;
    public float enmityThreshold;
    public float deitySpecialAttackPower;

    public DeityBehavior deityBehavior;

    public delegate void DeityJudgment();
    public static event DeityJudgment OnDeityJudgment;

    public delegate void DeityJudgmentCounterUpdate(int judgmentTurnLimitNumber);
    public static event DeityJudgmentCounterUpdate OnDeityJudgmentCounterUpdate;

    public delegate void DeityNotificationUpdate(string deityNotificationText);
    public static event DeityNotificationUpdate OnDeityNotificationUpdate;

    public delegate void DeityFieldEffectActivation();
    public static event DeityFieldEffectActivation OnDeityFieldEffectActivation;

    public delegate void PlayerTurnSwap();
    public static event PlayerTurnSwap OnPlayerTurnSwap;

    public delegate void PlayerTurn(string playerTurn);
    public static event PlayerTurn OnPlayerTurn;

    public delegate void DeitySpawn(GameObject deity);
    public static event DeitySpawn OnDeitySpawn;

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
        //OnDeitySpawn(this.gameObject);
        battleManager = GameObject.FindGameObjectWithTag("BattleManager").GetComponent<BattleManager>();
        OnDeityNotificationUpdate("The Deity is watching over the Battlefield");
        //OnDeityJudgmentCounterUpdate(judgmentTurnLimit);
        GameObject newDeityEnmityTracker = GameObject.FindGameObjectWithTag("TurnTrackerDetailsContainer");
        //Finds the Turn Tracker Details Panel where to instance the Deity Enmity Tracker
        newDeityEnmityTracker = Instantiate(deityEnmityTracker, newDeityEnmityTracker.transform);
        newDeityEnmityTracker.GetComponent<DeityEnmityTrackerController>().SetDeity(this.gameObject);
        newDeityEnmityTracker.GetComponent<DeityEnmityTrackerController>().UpdateDeityEnmityTracker();
        deityEnmityTracker = newDeityEnmityTracker;
    }
    public void DeityBehaviour()
    //A simple method that informs the Deity logic. Needs refactoring after prototyping.
    //Consider changing the current implementation with a system based on Switch/Case.
    {
        StartCoroutine("EndDeityTurn");
        Debug.Log("Deity Behaviour");
        deityBehavior.ExecuteBehavior(this);
    }
    /*

    if (PerformDeityEnmityCheck())
    {
        //currentDeityState = DeityState.indifferent;
        GameObject newDeityAttackVFX = Instantiate(deityAttackVFX, transform);
        Destroy(newDeityAttackVFX, 3);
        Debug.Log("Deity Attacks");
        /*GameObject[] playerUnitsOnBattlefield = GameObject.FindGameObjectWithTag("PlayerPartyController").GetComponent<PlayerPartyController>().playerUnitsOnBattlefield;
        foreach (var playerUnit in playerUnitsOnBattlefield)
        {
            playerUnit.GetComponent<Unit>().unitHealthPoints -= deitySpecialAttackPower;
        }
        *
        deityBehavior.ExecuteBehavior(this);
        enmity = 0;
        deityEnmityTracker.GetComponent<DeityEnmityTrackerController>().UpdateDeityEnmityTracker();
    }
    else
    {
        Debug.Log("Deity doesn't do anything");
    }*/


    /*public void DeityAttack()
    //A simple attack from the Deity that deals damage to the Player after checking it's accrued enmity.
    {
        if (deityAttackVFX != null)
        {
            deityAttackVFX.SetActive(true);
        }
        player.healthPoints -= 10 * player.GetComponent<SinTracker>().redEnmity;
        player.UpdatePlayerHealthDisplay();
        OnDeityNotificationUpdate("The Deity has Attacked");
        StartCoroutine("ResetDeityAttack");
    }
    */
    IEnumerator EndDeityTurn()
    {
        //OnDeityJudgmentCounterUpdate(judgmentTurnLimit);
        yield return new WaitForSeconds(1f);
        Debug.Log("Ending Deity Turn");
        OnPlayerTurnSwap();
        OnPlayerTurn("Player Turn");
    }
    public bool PerformDeityEnmityCheck()
    {
        if (enmity >= enmityThreshold)
        {
            //Deity Attacks
            return true;
        }
        else

        {
            //Deity doesn't Attack
            return false;
        }
    }
}

