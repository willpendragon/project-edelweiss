using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class BattleInterface : MonoBehaviour
{
    public static BattleInterface Instance { get; private set; }

    [Header("Logic")]
    [SerializeField] BattleManager battleManager;
    //[SerializeField] Player player;
    [SerializeField] float battlefieldNotificationsPanelDurationTime;
    //[SerializeField] GameObject playerActionPanel;
    public SummonedUnitInfoPanelHelper summonedUnitInfoPanelHelper;

    [Header("UI Elements")]
    [SerializeField] Image moveNamePanel;
    //[SerializeField] Image fieldEffectIcon;
    [SerializeField] RectTransform battlefieldNotificationsPanel;
    [SerializeField] public GameObject movesContainer;
    [SerializeField] CanvasGroup fadePanel;
    public BattleMomentsScreenHelper battleMomentsScreenHelper;
    [SerializeField] RectTransform summonedUnitsInfoContainer;

    [Header("UI Texts")]
    [SerializeField] TextMeshProUGUI moveName;
    [SerializeField] TextMeshProUGUI playerActionText;
    [SerializeField] TextMeshProUGUI battlefieldTextNotifications;
    //[SerializeField] TextMeshProUGUI deityJudgmentLimitText;

    [Header("UI Objects")]
    [SerializeField] GameObject summonedUnitInfoPanel;

    public TextMeshProUGUI battleEndResult;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void OnEnable()
    {
        Deity.OnDeityNotificationUpdate += SetDeityNotification;
        AOESpellPlayerAction.OnUsedSpell += SetSpellNameOnNotificationPanel;
        MeleePlayerAction.OnUsedMeleeAction += SetMeleeAttackOnNotificationPanel;
        BumperEnemyBehavior.OnBumperEnemyAttack += SetMeleeAttackOnNotificationPanel;
        StunnerEnemyBehavior.OnStunnerEnemyAttack += SetMeleeAttackOnNotificationPanel;
        MoonPhaseController.OnMoonPhaseSwitch += SetMoonNotification;
        MoonPhaseController.OnMoonPhaseBuffActivation += SetMoonNotification;
        MirrorController.OnMirrorAttack += SetMirrorNotification;
        BossSimildeBehaviour.OnBossEnemyAttack += SetMeleeAttackOnNotificationPanel;
    }
    private void OnDisable()
    {
        Deity.OnDeityNotificationUpdate -= SetDeityNotification;
        AOESpellPlayerAction.OnUsedSpell -= SetSpellNameOnNotificationPanel;
        MeleePlayerAction.OnUsedMeleeAction -= SetMeleeAttackOnNotificationPanel;
        BumperEnemyBehavior.OnBumperEnemyAttack -= SetMeleeAttackOnNotificationPanel;
        StunnerEnemyBehavior.OnStunnerEnemyAttack -= SetMeleeAttackOnNotificationPanel;
        MoonPhaseController.OnMoonPhaseSwitch -= SetMoonNotification;
        MoonPhaseController.OnMoonPhaseBuffActivation -= SetMoonNotification;
        MirrorController.OnMirrorAttack -= SetMirrorNotification;
        BossSimildeBehaviour.OnBossEnemyAttack -= SetMeleeAttackOnNotificationPanel;
    }
    private void Start()
    {
        FadeIn();
    }
    public void FadeIn()
    {
        float duration = 0.5f;
        if (fadePanel != null)
        {
            // Animate the CanvasGroup alpha to 0 (fully transparent)
            fadePanel.DOFade(0, duration);

            fadePanel.interactable = false;
            fadePanel.blocksRaycasts = false;
        }
    }

    public void SetSpellNameOnNotificationPanel(string spellName, string casterName)
    {
        battlefieldNotificationsPanel.transform.localScale = new Vector3(1, 1, 1);
        battlefieldTextNotifications.text = casterName + " used " + spellName;
        StartCoroutine("ResetBattleFieldTextNotification");
    }

    public void SetSummonEffectNameOnNotificationPanel(string summonName, string unitName)
    {
        battlefieldNotificationsPanel.transform.localScale = new Vector3(1, 1, 1);
        battlefieldTextNotifications.text = summonName + " blessed " + unitName;
        StartCoroutine("ResetBattleFieldTextNotification");
    }
    public void SetMeleeAttackOnNotificationPanel(string meleeAttack, string attackerName)
    {
        battlefieldNotificationsPanel.transform.localScale = new Vector3(1, 1, 1);
        battlefieldTextNotifications.text = attackerName + " used " + meleeAttack;
        StartCoroutine("ResetBattleFieldTextNotification");
    }
    public void SetDeityNotification(string deityNotification)
    {
        battlefieldNotificationsPanel.transform.localScale = new Vector3(1, 1, 1);
        battlefieldTextNotifications.text = deityNotification;
        StartCoroutine("ResetBattleFieldTextNotification");
    }

    public void SetMoonNotification(string moonNotification)
    {
        battlefieldNotificationsPanel.transform.localScale = new Vector3(1, 1, 1);
        battlefieldTextNotifications.text = moonNotification;
        StartCoroutine("ResetBattleFieldTextNotification");
    }
    public void SetMirrorNotification(string mirrorNotification)
    {
        battlefieldNotificationsPanel.transform.localScale = new Vector3(1, 1, 1);
        battlefieldTextNotifications.text = mirrorNotification;
        StartCoroutine("ResetBattleFieldTextNotification");
    }
    IEnumerator ResetBattleFieldTextNotification()
    {
        yield return new WaitForSeconds(battlefieldNotificationsPanelDurationTime);
        battlefieldNotificationsPanel.transform.localScale = new Vector3(0, 0, 0);
    }
    public void CreateUISummonInfoPanel(GameObject deityGameObject)
    {
        float deityPrayerBuffThreshold = deityGameObject.GetComponent<Deity>().deityPrayerBuff.deityPrayerBuffThreshold;
        float deityPrayerPower = deityGameObject.GetComponent<Deity>().deityPrayerPower;
        string deityName = deityGameObject.GetComponent<Unit>().unitTemplate.unitName;
        GameObject newSummonedUnitInfoPanel = Instantiate(summonedUnitInfoPanel, summonedUnitsInfoContainer);
        SummonedUnitInfoPanelHelper summonedUnitInfoPanelHelper = newSummonedUnitInfoPanel.GetComponent<SummonedUnitInfoPanelHelper>();
        summonedUnitInfoPanelHelper.SetSummonedUnitInfoPanelValues(deityName, deityPrayerBuffThreshold, deityPrayerPower);
    }
    public void DestroyUISummonInfoPanel()
    {
        Destroy(summonedUnitInfoPanel, 1);
    }
}