using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchTilesToDeitySummoningAreaSelectionMode : MonoBehaviour
{
    public delegate void SummonButtonPressed();
    public static event SummonButtonPressed OnSummonButtonPressed;

    public void PressSummonButton()
    {
        OnSummonButtonPressed();
    }

}
