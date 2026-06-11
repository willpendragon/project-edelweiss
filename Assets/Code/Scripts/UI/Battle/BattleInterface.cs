
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;

public class BattleInterface : MonoBehaviour // must be renamed to BattleUIManager
{
    public static BattleInterface Instance { get; private set; }

    [Header("Logic")]
    [SerializeField] BattleManager battleManager;
    [SerializeField] private PlayerPartyProfilesUIManager _playerPartyProfilesUIManager;
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

    public PlayerPartyProfilesUIManager PlayerPartyProfilesUIManager => _playerPartyProfilesUIManager;

    private Tween activeNotificationTween;
    private Queue<string> _notificationQueue = new Queue<string>();
    private bool _isDisplayingNotification = false;

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
        PhysicalAttackBehavior.OnUsedPhysicalAttack += SetBattleNotification;
        PlaceCrystalPlayerAction.OnPlaceCrystal += SetBattleNotification;
        TrapPlayerAction.OnNotEnoughMana += SetBattleNotification;
        BumperEnemyBehavior.OnBumperEnemyAttack += SetBattleNotification;
        BumperEnemyBehavior.OnMovementDisabled += SetBattleNotification;
        StunnerEnemyBehavior.OnStunnerEnemyAttack += SetBattleNotification;
        RockEnemyBehavior.OnRockEnemyAttack += SetBattleNotification;
        PrizeCollectionHelper.OnUpgradeObtained += SetBattleNotification;
        UnitSelectionController.OnFaithlessUnit += SetBattleNotification;
    }

    private void UnsubscribeBattleNotifications()
    {
        RadialMenuEntry.OnPointsDepleted -= SetBattleNotification;
        MovePlayerAction.OnUnitNegativeStatus -= SetBattleNotification;
        AOESpellPlayerAction.OnUsedSpell -= SetBattleNotification;
        AOESpellPlayerAction.OnNotEnoughMana -= SetBattleNotification;
        PhysicalAttackBehavior.OnUsedPhysicalAttack -= SetBattleNotification;
        PlaceCrystalPlayerAction.OnPlaceCrystal -= SetBattleNotification;
        TrapPlayerAction.OnNotEnoughMana -= SetBattleNotification;
        BumperEnemyBehavior.OnBumperEnemyAttack -= SetBattleNotification;
        BumperEnemyBehavior.OnMovementDisabled -= SetBattleNotification;
        StunnerEnemyBehavior.OnStunnerEnemyAttack -= SetBattleNotification;
        RockEnemyBehavior.OnRockEnemyAttack -= SetBattleNotification;
        PrizeCollectionHelper.OnUpgradeObtained -= SetBattleNotification;
        UnitSelectionController.OnFaithlessUnit -= SetBattleNotification;
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
        // Add the new message to the queue
        _notificationQueue.Enqueue(message);

        // If we aren't currently showing a message, start the display loop
        if (!_isDisplayingNotification)
        {
            DisplayNextNotification();
        }
    }

    private void DisplayNextNotification()
    {
        // If the queue is empty, hide the panel and stop
        if (_notificationQueue.Count == 0)
        {
            _isDisplayingNotification = false;
            if (activeNotificationTween != null && activeNotificationTween.IsActive())
            {
                activeNotificationTween.Kill();
            }
            battlefieldNotificationsPanel.transform.localScale = Vector3.zero;
            return;
        }

        // We are currently showing a message
        _isDisplayingNotification = true;

        // Get the next message
        string message = _notificationQueue.Dequeue();

        if (activeNotificationTween != null && activeNotificationTween.IsActive())
        {
            activeNotificationTween.Kill();
        }

        battlefieldNotificationsPanel.transform.localScale = Vector3.one;
        battlefieldTextNotifications.text = message;
        
        // Add a tiny, quick "pop" animation so the player notices the text changed instantly
        battlefieldNotificationsPanel.DOPunchScale(new Vector3(0.1f, 0.1f, 0.0f), 0.15f, 0, 0);
        BattleSFXManager.PlaySound(SoundType.POPUPMESSAGE, 1);
        
        // Wait for the duration, then automatically trigger the next notification in the queue
        activeNotificationTween = DOVirtual.DelayedCall(battlefieldNotificationsPanelDurationTime, () =>
        {
            DisplayNextNotification();
        });
    }
}