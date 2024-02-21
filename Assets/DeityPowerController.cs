using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeityPowerController : MonoBehaviour
{
    private void OnEnable()
    {
        PrayPlayerAction.OnPlayerPrayer += IncreaseDeityPower;
    }
    private void OnDisable()
    {
        PrayPlayerAction.OnPlayerPrayer -= IncreaseDeityPower;
    }
    private void IncreaseDeityPower()
    {
        Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();
        activePlayerUnit.linkedDeity.deityPrayerPower++;
    }

}
