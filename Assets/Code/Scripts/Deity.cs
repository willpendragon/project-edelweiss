using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using static Deity;

/*public enum alignmentType
{
    red,
    blue
}

public enum blackListedAlignment
{
    red,
    blue
}

*/
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
    public Player player;
    public TextMeshProUGUI deityAttackNotification;
    public BattleManager battleManager;
    public BattleInterface battleInterface;
    public float enmity;
    [SerializeField] GameObject deityAttackVFX;
    [SerializeField] GameObject deityFieldEffectVFX;
    public int judgmentTurnLimit;
    public DeityState currentDeityState;
    //public int judgmentAttackPower = 1000;
    public List<SpellAlignment> hatedSpellAlignments;
    public GameObject deityEnmityTracker;
    public float enmityThreshold;
    public float deitySpecialAttackPower;

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
        //TileController.OnJudgmentAttackSuccessful += ApplyJudgmentAttackDamage;
    }
    private void OnDisable()
    {
        EnemyTurnManager.OnDeityTurn -= DeityBehaviour;
        //TileController.OnJudgmentAttackSuccessful -= ApplyJudgmentAttackDamage;
    }

    public void Start()
    {
        //OnDeitySpawn(this.gameObject);
        battleManager = GameObject.FindGameObjectWithTag("BattleManager").GetComponent<BattleManager>();
        OnDeityNotificationUpdate("The Deity is watching over the Battlefield");
        OnDeityJudgmentCounterUpdate(judgmentTurnLimit);
        GameObject newDeityEnmityTracker = GameObject.FindGameObjectWithTag("TurnTrackerDetailsContainer");
        //Finds the Turn Tracker Details Panel where to instance the Deity Enmity Tracker
        newDeityEnmityTracker = Instantiate(deityEnmityTracker, newDeityEnmityTracker.transform);
        newDeityEnmityTracker.GetComponent<DeityEnmityTrackerController>().SetDeity(this.gameObject);
        newDeityEnmityTracker.GetComponent<DeityEnmityTrackerController>().UpdateDeityEnmityTracker();
        deityEnmityTracker = newDeityEnmityTracker;
    }

    /*public void Update()
    {
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
    */

    public void DeityBehaviour()
    //A simple method that informs the Deity logic. Needs refactoring after prototyping.
    //Consider changing the current implementation with a system based on Switch/Case.
    {
        StartCoroutine("EndDeityTurn");
        Debug.Log("Deity Behaviour");
        if (PerformDeityEnmityCheck())
        {
            //currentDeityState = DeityState.indifferent;
            GameObject newDeityAttackVFX = Instantiate(deityAttackVFX, transform);
            Destroy(newDeityAttackVFX, 3);
            Debug.Log("Deity Attacks");
            GameObject[] playerUnitsOnBattlefield = GameObject.FindGameObjectWithTag("PlayerPartyController").GetComponent<PlayerPartyController>().playerUnitsOnBattlefield;
            foreach (var playerUnit in playerUnitsOnBattlefield)
            {
                playerUnit.GetComponent<Unit>().unitHealthPoints -= deitySpecialAttackPower;
            }
            enmity = 0;
            deityEnmityTracker.GetComponent<DeityEnmityTrackerController>().UpdateDeityEnmityTracker();
        }
        else
        {
            Debug.Log("Deity doesn't do anything");
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
        OnDeityNotificationUpdate("The Deity has Attacked");
        StartCoroutine("ResetDeityAttack");
    }

    /*public void DeityFieldEffect()
    //A simple effect provoked by the Deity that affects the Units on the Battlefield with varying effects.
    {
        /*if (deityFieldEffectVFX != null)
        {
            deityFieldEffectVFX.SetActive(true);
        }
        
        OnDeityFieldEffectActivation();
        OnDeityNotificationUpdate("The Deity has created a Field Effect");
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
        OnDeityNotificationUpdate("The Deity has created a Field Effect");
        //deityAttackNotification.text = "";
        //battleManager.turnCounter = 0;
        //battleManager.turnTracker.text = "0";
    }
    */
    IEnumerator EndDeityTurn()
    {
        //judgmentTurnLimit--;
        OnDeityJudgmentCounterUpdate(judgmentTurnLimit);
        yield return new WaitForSeconds(1f);
        Debug.Log("Ending Deity Turn");
        OnPlayerTurnSwap();
        OnPlayerTurn("Player Turn");
    }

    /*public void ApplyJudgmentAttackDamage()
    {
        player.GetComponent<Player>().healthPoints = judgmentAttackPower;
        //Multiply for the Enmity Value the Player accrued towards this Deity
    }
    */
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

