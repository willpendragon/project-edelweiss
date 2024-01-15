using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleInterface : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI moveName;
    public TextMeshProUGUI battleEndResult;
    [SerializeField] Image moveNamePanel;
    [SerializeField] Image fieldEffectIcon;
    [SerializeField] BattleManager battleManager;
    [SerializeField] TextMeshProUGUI playerActionText;
    [SerializeField] GameObject playerActionPanel;
    [SerializeField] TextMeshProUGUI battlefieldTextNotifications;
    [SerializeField] TextMeshProUGUI deityJudgmentLimitText;
    [SerializeField] Player player;

    private void OnEnable()
    {
        Moveset.OnPlayerChangesPosition += ChangePlayerActionModeText;
        Moveset.OnPlayerMovementModeEnd += DeactivatePlayerActionModePanel;
        Deity.OnDeityJudgmentCounterUpdate += SetDeityJudgmentCounter;
        Deity.OnDeityNotificationUpdate += SetDeityNotification;
        Deity.OnDeityFieldEffectActivation += ShowFieldEffectIcon;
        SpellcastingController.OnCastedSpellTextNotification += SetSpellNameOnNotificationPanel;
    }
    private void OnDisable()
    {
        Moveset.OnPlayerChangesPosition -= ChangePlayerActionModeText;
        Moveset.OnPlayerMovementModeEnd -= DeactivatePlayerActionModePanel;
        Deity.OnDeityJudgmentCounterUpdate -= SetDeityJudgmentCounter;
        Deity.OnDeityNotificationUpdate -= SetDeityNotification;
        Deity.OnDeityFieldEffectActivation -= ShowFieldEffectIcon;
        SpellcastingController.OnCastedSpellTextNotification -= SetSpellNameOnNotificationPanel;
    }
    private void Start()
    {
        player.UpdatePlayerHealthDisplay();
        player.UpdatePlayerShieldDisplay();
        player.UpdateManaPointsDisplay();
        Debug.Log("Test Moveset Bug");
    }
    /*public void SetMovePanelName(string currentMoveName)
    {
        moveName.text = currentMoveName;
        moveNamePanel.color = new Color(1, 1, 1, 1);
        StartCoroutine("ResetMovePanel");
    }
    */

    public void SetSpellNameOnNotificationPanel(string spellName)
    {
        battlefieldTextNotifications.text = spellName;
    }

    public void SetDeityJudgmentCounter(int judgmentTurnLimitNumber)
    {
        deityJudgmentLimitText.text = judgmentTurnLimitNumber.ToString();
        Debug.Log("Setting Judgment Turn Warning");
    }
    public void SetDeityNotification(string deityNotification)
    {
        battlefieldTextNotifications.text = deityNotification;
    }

    IEnumerator ResetMovePanel()
    {
        yield return new WaitForSeconds(2);
        moveName.text = "";
        moveNamePanel.color = new Color(1, 1, 1, 0);
    }

    public void ShowFieldEffectIcon()
    {
        fieldEffectIcon.color = new Color(255, 255, 255, 255);
    }

    public void ChangePlayerActionModeText()
    {
        playerActionPanel.SetActive(true);
        playerActionText.text = "Movement Mode";
    }

    public void DeactivatePlayerActionModePanel()
    {
        battlefieldTextNotifications.text = "Player Is Unable to Move";
        playerActionPanel.SetActive(false);
    }
    /*
    public void JudgmentAttackFailed()
    {
        battleEndResult.text = "The Player escaped";
        GameManager gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        gameManager.MarkCurrentNodeAsCompleted();
    }
    */
}