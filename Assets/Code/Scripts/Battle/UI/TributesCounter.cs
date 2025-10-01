using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TributesCounter : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _tributesCounter;
    public void UpdateTributesCounter()
    {
        _tributesCounter.text = $"<sprite=98> Tributes Counter\n{BattleManager.Instance.captureCrystalsRewardPool}";
    }
}
