using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;

public class BattleInterface : MonoBehaviour
{
    public static BattleInterface Instance { get; private set; }

    [Header("Logic")]
    [SerializeField] BattleManager battleManager;
    [SerializeField] float battlefieldNotificationsPanelDurationTime;
    public SummonedUnitInfoPanelHelper summonedUnitInfoPanelHelper;

    [Header("UI Elements")]
    [SerializeField] Image moveNamePanel;
    [SerializeField] RectTransform battlefieldNotificationsPanel;
    [SerializeField] CanvasGroup fadePanel;
    public BattleMomentsScreenHelper battleMomentsScreenHelper;
    [SerializeField] RectTransform summonedUnitsInfoContainer;
    public GameObject battleDetails;

    [Header("UI Texts")]
    [SerializeField] TextMeshProUGUI moveName;
    [SerializeField] TextMeshProUGUI playerActionText;
    [SerializeField] TextMeshProUGUI battlefieldTextNotifications;

    [Header("UI Objects")]
    [SerializeField] GameObject summonedUnitInfoPanel;

    public TextMeshProUGUI battleEndResult;

    public delegate void DeselectPlayerAction();
    public static event DeselectPlayerAction OnDeselectPlayerAction;

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

    private void SubscribeBattleNotifications()
    {
        RadialMenuEntry.OnPointsDepleted += SetBattleNotification;
        MovePlayerAction.OnUnitNegativeStatus += SetBattleNotification;
        AOESpellPlayerAction.OnUsedSpell += SetBattleNotification;
        AOESpellPlayerAction.OnNotEnoughMana += SetBattleNotification;
        MeleePlayerAction.OnUsedMeleeAction += SetBattleNotification;
        BumperEnemyBehavior.OnBumperEnemyAttack += SetBattleNotification;
        BumperEnemyBehavior.OnMovementDisabled += SetBattleNotification;
        StunnerEnemyBehavior.OnStunnerEnemyAttack += SetBattleNotification;
        DeityBehavior.OnUsedAttack += SetBattleNotification;
    }

    private void UnsubscribeBattleNotifications()
    {
        RadialMenuEntry.OnPointsDepleted -= SetBattleNotification;
        MovePlayerAction.OnUnitNegativeStatus -= SetBattleNotification;
        AOESpellPlayerAction.OnUsedSpell -= SetBattleNotification;
        AOESpellPlayerAction.OnNotEnoughMana -= SetBattleNotification;
        MeleePlayerAction.OnUsedMeleeAction -= SetBattleNotification;
        BumperEnemyBehavior.OnBumperEnemyAttack -= SetBattleNotification;
        BumperEnemyBehavior.OnMovementDisabled -= SetBattleNotification;
        StunnerEnemyBehavior.OnStunnerEnemyAttack -= SetBattleNotification;
        DeityBehavior.OnUsedAttack -= SetBattleNotification;
    }

    private void OnEnable()
    {
        Deity.OnDeityNotificationUpdate += SetDeityNotification;
        PlaceCrystalPlayerAction.OnCaptureAttempt += SetDeityNotification;
        SelectUnitPlayerAction.OnFaithlessCharacter += SetFaithlessCharacterNotification;
        SubscribeBattleNotifications();
    }
    private void OnDisable()
    {
        Deity.OnDeityNotificationUpdate -= SetDeityNotification;
        PlaceCrystalPlayerAction.OnCaptureAttempt -= SetDeityNotification;
        SelectUnitPlayerAction.OnFaithlessCharacter -= SetFaithlessCharacterNotification;
        UnsubscribeBattleNotifications();
    }

    // Add Fade In using FadeCanvas

    //private void Start()
    //{
    //    FadeIn();
    //}
    //public void FadeIn()
    //{
    //    float duration = 0.5f;
    //    if (fadePanel != null)
    //    {
    //        fadePanel.DOFade(0, duration);

    //        fadePanel.interactable = false;
    //        fadePanel.blocksRaycasts = false;
    //    }
    //}

    public void SetBattleNotification(string actionNotification)
    {
        ShowNotification(actionNotification);
    }

    public void SetSummonEffectNameOnNotificationPanel(string summonName, string unitName)
    {
        ShowNotification($"{summonName} blessed {unitName}");
    }
    public void SetDeityNotification(string deityNotification)
    {
        ShowNotification(deityNotification);
        DOTween.Sequence()
            .AppendInterval(0.5f)
            .AppendCallback(() => battlefieldNotificationsPanel.transform.localScale = Vector3.one);
    }

    public void SetFaithlessCharacterNotification(string faithlessCharacterNotification)
    {
        ShowNotification(faithlessCharacterNotification);
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
    public void DeactivateActionInfoPanel()
    {
        OnDeselectPlayerAction();
    }

    private void ShowNotification(string message)
    {
        battlefieldNotificationsPanel.transform.localScale = Vector3.one;
        battlefieldTextNotifications.text = message;
        StartCoroutine(ResetBattleFieldTextNotification());
    }
    IEnumerator ResetBattleFieldTextNotification()
    {
        yield return new WaitForSeconds(battlefieldNotificationsPanelDurationTime);
        battlefieldNotificationsPanel.transform.localScale = new Vector3(0, 0, 0);
    }
}