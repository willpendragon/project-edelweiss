using UnityEngine;

public class DeityPowerController : MonoBehaviour
{
    public delegate void PlayerUnitPraying();
    public static event PlayerUnitPraying OnPlayerUnitPraying;
    private void OnEnable()
    {
        SummonedUnitInfoPanelHelper.OnPlayerPrayer += IncreaseDeityPower;
    }
    private void OnDisable()
    {
        SummonedUnitInfoPanelHelper.OnPlayerPrayer -= IncreaseDeityPower;
    }
    private void IncreaseDeityPower()
    {
        Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();
        if (activePlayerUnit != null && activePlayerUnit.summonedLinkedDeity != null)
        {
            if (activePlayerUnit.summonedLinkedDeity.deityPrayerPower < activePlayerUnit.summonedLinkedDeity.deityPrayerBuff.deityPrayerBuffThreshold)
            {
                activePlayerUnit.summonedLinkedDeity.deityPrayerPower++;
                OnPlayerUnitPraying();
                Debug.Log("Deity Power Increases");
            }
            else
            {
                Debug.Log("Deity Prayer Power is already at Max Capacity");
            }
        }
    }
}