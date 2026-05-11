using ProjectEdelweiss.Utils;
using UnityEngine;

public class DeityPowerController : MonoBehaviour
{
    public delegate void PlayerUnitPraying();

    public static event PlayerUnitPraying OnPlayerUnitPraying;

    private void OnEnable()
    {
        // SummonedUnitInfoPanelHelper.OnPlayerPrayer += IncreaseDeityPower;
    }

    private void OnDisable()
    {
        // SummonedUnitInfoPanelHelper.OnPlayerPrayer -= IncreaseDeityPower;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            UseDeityMove();
        }
    }

    public void UseDeityMove()
    {
        Unit activePlayerUnit = GameObject.FindGameObjectWithTag(GameTags.ActivePlayerUnit).GetComponent<Unit>();
        if (activePlayerUnit == null)
            return;
        Deity deity = activePlayerUnit.linkedDeity;
        if (deity == null)
            return;
        deity.summoningBehaviour.ExecuteBehavior(deity);
    }
}