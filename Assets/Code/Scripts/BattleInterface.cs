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

    private void OnEnable()
    {
        Moveset.OnPlayerChangesPosition += ChangePlayerActionModeText;
        Moveset.OnPlayerMovementModeEnd += DeactivatePlayerActionModePanel;
    }
    private void OnDisable()
    {
        Moveset.OnPlayerChangesPosition -= ChangePlayerActionModeText;
        Moveset.OnPlayerMovementModeEnd -= DeactivatePlayerActionModePanel;
    }
    public void SetMovePanelName(string currentMoveName)
    {
        moveName.text = currentMoveName;
        moveNamePanel.color = new Color(1, 1, 1, 1);
        StartCoroutine("ResetMovePanel");
    }

    IEnumerator ResetMovePanel()
    {
        yield return new WaitForSeconds(2);
        moveName.text = "";
        moveNamePanel.color = new Color(1, 1, 1, 0);
    }

    public void Update()
    {
        if (battleManager.fieldEffectStatus == FieldEffectStatus.active)
        {
            ShowFieldEffectIcon();
            Debug.Log("Show Field Effect Icon");
        }
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
}
