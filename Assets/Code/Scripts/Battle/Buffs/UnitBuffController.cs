
using System.Collections.Generic;
using UnityEngine;

public class UnitBuffController : MonoBehaviour
{
    public class AppliedBuffEntry
    {
        public FoodBuff.FoodBuffType Type;
        public float AppliedValue;
        public int RemainingDurationDays;
    }

    private Dictionary<FoodBuff.FoodBuffType, List<AppliedBuffEntry>> _appliedBuffs
        = new Dictionary<FoodBuff.FoodBuffType, List<AppliedBuffEntry>>();

    [SerializeField] Unit unitReference;

    public void CreateAppliedBuffEntry(float value, int duration, FoodBuff.FoodBuffType type)
    {
        var entry = new AppliedBuffEntry
        {
            Type = type,
            AppliedValue = value,
            RemainingDurationDays = duration
        };

        if (_appliedBuffs == null || _appliedBuffs.Count <= 0)
            return;

        if (!_appliedBuffs.TryGetValue(type, out var list))
        {
            list = new List<AppliedBuffEntry>();
            _appliedBuffs[type] = list;
        }

        list.Add(entry);
    }

    public void SubtractDurationDaysFromBuffEntries(int subtractedDays)
    {
        // Method to subtract duration days from all of the Buff entries.
        var typesToRemove = new List<FoodBuff.FoodBuffType>();

        foreach (var kvp in _appliedBuffs)
        {
            var buffType = kvp.Key;
            var list = kvp.Value;

            for (int i = list.Count - 1; i >= 0; i--)
            {
                var entry = list[i];
                entry.RemainingDurationDays -= subtractedDays;
                if (entry.RemainingDurationDays <= 0)
                {
                    BuffRemoval(entry);
                    list.RemoveAt(i);
                }
            }

            if (list.Count == 0)
            {
                typesToRemove.Add(buffType);
            }
        }
        // Clean empty lists.
        foreach (var type in typesToRemove)
        {
            _appliedBuffs.Remove(type);
        }
    }

    public void BuffRemoval(AppliedBuffEntry buffEntry)
    {
        switch (buffEntry.Type)
        {
            case FoodBuff.FoodBuffType.Attack:
                unitReference.unitAttackPower -= buffEntry.AppliedValue;
                break;
            case FoodBuff.FoodBuffType.Defense:
                unitReference.unitShieldPoints -= buffEntry.AppliedValue;
                break;
        }
    }
}