using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PastrySlotController : MonoBehaviour
{
    [SerializeField] public List<Unit> Units;
    [SerializeField] private Dictionary<Unit, List<ItemFood>> foodHistory = new();

    void Start()
    {
        Units = GameManager.Instance.playerPartyMembersInstances;
    }

    public void RegisterUnit(Unit unit)
    {
        if (!foodHistory.ContainsKey(unit))
        {
            foodHistory[unit] = new List<ItemFood>();
        }
    }

    public void TrackEatenFood(Unit unit, ItemFood itemFood)
    {
        if (!foodHistory.ContainsKey(unit))
            RegisterUnit(unit);

        foodHistory[unit].Add(itemFood);

        foreach (var pair in foodHistory)
        {
            Debug.Log($"{pair.Key.unitTemplate.unitName} has eaten: {string.Join(", ", pair.Value.Select(i => i.itemFoodName))}");
        }
    }

    public List<ItemFood> GetHistory(Unit unit)
    {
        if (foodHistory.TryGetValue(unit, out var history))
            return history;
        return new List<ItemFood>();
    }
}
