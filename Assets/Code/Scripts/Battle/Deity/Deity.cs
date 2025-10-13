using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class Deity : MonoBehaviour
{
    public enum DeityStatus
    {
        Standard,
        Summoned
    }

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
    [SerializeField] private float _maxEnmity = 10;
    //public float deityPrayerPowerThreshold;

    [Header("Visuals")]
    public TextMeshProUGUI deityAttackNotification;
    public BattleInterface battleInterface;
    public GameObject deityAttackVFX;
    public GameObject deityEnmityTracker;
    public Sprite deityPortrait;
    public GameObject deityHealthBar;
    [SerializeField] DeityEnmityTrackerController enmityTracker;

    public AudioSource deityCry;

    public DeityStatus currentDeityStatus = DeityStatus.Standard;


    public delegate void DeityNotificationUpdate(string deityNotificationText);
    public static event DeityNotificationUpdate OnDeityNotificationUpdate;

    public delegate void PlayerTurnSwap();
    public static event PlayerTurnSwap OnPlayerTurnSwap;

    public delegate void PlayerTurn(string playerTurn);
    public static event PlayerTurn OnPlayerTurn;

    public delegate void DeitySpawn(GameObject deity);
    public static event DeitySpawn OnDeitySpawn;

    [SerializeField] private Slider _enmityBar;

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

        var enemyTurnManager = FindAnyObjectByType<EnemyTurnManager>();
        enemyTurnManager.deity = this.gameObject;
        // Set Enmity Bar value
        _enmityBar.maxValue = _maxEnmity;
        _enmityBar.value = enmity;

        UpdateSinSystemDisplay();
    }

    // Retrieves the Deity Behavior from a compatible Scriptable Object added in the Inspector.
    public void DeityBehaviour(string deityText)
    {
        deityBehavior.ExecuteBehavior(this);
    }

    public void UpdateDeityHealthBar()
    {
        Unit deityUnitComponent = GetComponentInChildren<Unit>();
        deityHealthBar.GetComponentInChildren<Slider>().value = deityUnitComponent.unitHealthPoints;
    }

    public void UpdateDeityEnmitySlider()
    {
        _enmityBar.value = enmity;
        enmityTracker.PlayEnmityIconFeedback();
    }

    public void UpdateSinSystemDisplay()
    {
        var sinSystemDisplay = FindAnyObjectByType<SinSystemDisplay>();
        string deityName = transform.GetComponent<Unit>().unitTemplate.unitName;
        sinSystemDisplay.DisplaySinfulMoves(deityName, hatedSpellAlignments[0].ToString());
    }

    public bool PerformDeityEnmityCheck()
    {
        if (enmity >= _maxEnmity)
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

