using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SinSystemDisplay : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI sinSystemText;

    public void DisplaySinfulMoves(string deityName, string sinfulMoveName)
    {
        sinSystemText.text = $"{deityName} hates {sinfulMoveName}";
    }
}
