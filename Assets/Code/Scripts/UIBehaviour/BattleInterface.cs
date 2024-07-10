using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleInterface : MonoBehaviour
{
    public static BattleInterface Instance { get; private set; }

    [Header("Logic")]
    [SerializeField] BattleManager battleManager;
    //[SerializeField] Player player;
    [SerializeField] float battlefieldNotificationsPanelDurationTime;
    //[SerializeField] GameObject playerActionPanel;

    [Header("UI Visuals")]
    [SerializeField] Image moveNamePanel;
    //[SerializeField] Image fieldEffectIcon;
    [SerializeField] RectTransform battlefieldNotificationsPanel;
    [SerializeField] public GameObject movesContainer;


    [Header("UI Texts")]
    [SerializeField] TextMeshProUGUI moveName;
    [SerializeField] TextMeshProUGUI playerActionText;
    [SerializeField] TextMeshProUGUI battlefieldTextNotifications;
    //[SerializeField] TextMeshProUGUI deityJudgmentLimitText;

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
        //Deity.OnDeityJudgmentCounterUpdate += SetDeityJudgmentCounter;
        Deity.OnDeityNotificationUpdate += SetDeityNotification;
        AOESpellPlayerAction.OnUsedSpell += SetSpellNameOnNotificationPanel;
        MeleePlayerAction.OnUsedMeleeAction += SetMeleeAttackOnNotificationPanel;
        BumperEnemyBehavior.OnBumperEnemyAttack += SetMeleeAttackOnNotificationPanel;
        StunnerEnemyBehavior.OnStunnerEnemyAttack += SetMeleeAttackOnNotificationPanel;
    }
    private void OnDisable()
    {
        //Deity.OnDeityJudgmentCounterUpdate -= SetDeityJudgmentCounter;
        Deity.OnDeityNotificationUpdate -= SetDeityNotification;
        AOESpellPlayerAction.OnUsedSpell -= SetSpellNameOnNotificationPanel;
        MeleePlayerAction.OnUsedMeleeAction -= SetMeleeAttackOnNotificationPanel;
        BumperEnemyBehavior.OnBumperEnemyAttack -= SetMeleeAttackOnNotificationPanel;
        StunnerEnemyBehavior.OnStunnerEnemyAttack -= SetMeleeAttackOnNotificationPanel;
    }

    public void SetSpellNameOnNotificationPanel(string spellName, string casterName)
    {
        battlefieldNotificationsPanel.transform.localScale = new Vector3(1, 1, 1);
        battlefieldTextNotifications.text = casterName + " used " + spellName;
        StartCoroutine("ResetBattleFieldTextNotification");
    }
    public void SetMeleeAttackOnNotificationPanel(string meleeAttack, string attackerName)
    {
        battlefieldNotificationsPanel.transform.localScale = new Vector3(1, 1, 1);
        battlefieldTextNotifications.text = attackerName + " used " + meleeAttack;
        StartCoroutine("ResetBattleFieldTextNotification");
    }

    //public void SetDeityJudgmentCounter(int judgmentTurnLimitNumber)
    //{
    //    deityJudgmentLimitText.text = judgmentTurnLimitNumber.ToString();
    //    Debug.Log("Setting Judgment Turn Warning");
    //}
    public void SetDeityNotification(string deityNotification)
    {
        battlefieldNotificationsPanel.transform.localScale = new Vector3(1, 1, 1);
        battlefieldTextNotifications.text = deityNotification;
        StartCoroutine("ResetBattleFieldTextNotification");
    }
    IEnumerator ResetBattleFieldTextNotification()
    {
        yield return new WaitForSeconds(battlefieldNotificationsPanelDurationTime);
        battlefieldNotificationsPanel.transform.localScale = new Vector3(0, 0, 0);
    }

}