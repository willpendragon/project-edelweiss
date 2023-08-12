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
public void SetMovePanelName(string currentMoveName)
{
    moveName.text = currentMoveName;
    moveNamePanel.color = new Color (1,1,1,1);
    StartCoroutine("ResetMovePanel");
}

IEnumerator ResetMovePanel()
{
    yield return new WaitForSeconds(2);
    moveName.text = "";
    moveNamePanel.color = new Color (1,1,1,0);
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
    fieldEffectIcon.color = new Color (255,255,255,255);
}
}
