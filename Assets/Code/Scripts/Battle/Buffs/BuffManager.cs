using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffManager : MonoBehaviour
{
    [SerializeField] List<Unit> units = new List<Unit>();

    private void Start()
    {
        units = GameManager.Instance.playerPartyMembersInstances;
    }

    public void UpdateBuffs(int days)
    {
        foreach (var unit in units)
        {
            unit.GetComponent<UnitBuffController>().SubtractDurationDaysFromBuffEntries(days);
        }
    }
}
