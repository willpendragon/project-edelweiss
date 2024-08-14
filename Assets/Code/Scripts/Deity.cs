using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class Deity : MonoBehaviour
{
    [Header("Gameplay Logic")]
    public string Id = System.Guid.NewGuid().ToString();
    public float enmity;
    public List<SpellAlignment> hatedSpellAlignments;
    public float enmityThreshold;

    public DeityBehavior deityBehavior;
    public DeityBehavior summoningBehaviour;
    public DeityPrayerBuff deityPrayerBuff;


    public BattleManager battleManager;

    [Header("Deity Stats")]
    public float deitySpecialAttackPower;
    public float summoningPrice = 50;
    public float deityPrayerPower;
    //public float deityPrayerPowerThreshold;

    [Header("Visuals")]
    public TextMeshProUGUI deityAttackNotification;
    public BattleInterface battleInterface;
    public GameObject deityAttackVFX;
    public GameObject deityEnmityTracker;
    public Sprite deityPortrait;
    public GameObject deityHealthBar;

    public AudioSource deityCry;


    public delegate void DeityNotificationUpdate(string deityNotificationText);
    public static event DeityNotificationUpdate OnDeityNotificationUpdate;

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
        battleManager = GameObject.FindGameObjectWithTag("BattleManager").GetComponent<BattleManager>();
        OnDeityNotificationUpdate("The Deity is watching over the Battlefield");
        GameObject newDeityEnmityTracker = GameObject.FindGameObjectWithTag("TurnTrackerDetailsContainer");

        //Finds the Turn Tracker Details Panel where to instance the Deity Enmity Tracker
        newDeityEnmityTracker = Instantiate(deityEnmityTracker, newDeityEnmityTracker.transform);
        newDeityEnmityTracker.GetComponent<DeityEnmityTrackerController>().SetDeity(this.gameObject);
        newDeityEnmityTracker.GetComponent<DeityEnmityTrackerController>().UpdateDeityEnmityTracker();
        deityEnmityTracker = newDeityEnmityTracker;
    }

    //Retrieves the Deity Behavior from a compatible Scriptable Object added in the Inspector.
    public void DeityBehaviour(string deityText)
    {
        Debug.Log("Deity Behaviour Starts");
        StartCoroutine("EndDeityTurn");
        deityBehavior.ExecuteBehavior(this);
    }

    public void UpdateDeityHealthBar()
    {
        Unit deityUnitComponent = GetComponentInChildren<Unit>();
        deityHealthBar.GetComponentInChildren<Slider>().value = deityUnitComponent.unitHealthPoints;
    }
    IEnumerator EndDeityTurn()
    {
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

