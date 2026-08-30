using System.Collections.Generic;
using UnityEngine;

public class PlayerDebugBuffController : MonoBehaviour
{
    private const float BUFF_VALUE = 99999f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            BuffAllPlayerUnits();
        }
    }

    private void BuffAllPlayerUnits()
    {
        List<Unit> playerPartyMemberInstances = GameManager.Instance?.playerPartyMembersInstances;
        if (playerPartyMemberInstances == null)
            return;

        foreach (var unit in playerPartyMemberInstances)
        {
            if (unit == null)
                continue;

            // Set as raw fields to skip the HealthPoints property's death-check/event logic.
            unit.unitHealthPoints = BUFF_VALUE;
            unit.unitManaPoints = BUFF_VALUE;
            unit.unitOpportunityPoints = (int)BUFF_VALUE;
            unit.unitAttackPower = BUFF_VALUE;
            unit.unitMagicPower = BUFF_VALUE;
        }
    }
}
