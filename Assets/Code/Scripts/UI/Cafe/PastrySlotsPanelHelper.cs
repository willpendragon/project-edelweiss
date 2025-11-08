using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PastrySlotsPanelHelper : MonoBehaviour
{
    [SerializeField] private List<ItemFood> _eatenPastry;
    public void UpdatePastrySlots(Unit unit)
    {
        _eatenPastry.Clear();
        _eatenPastry = CafeMenuUIController.Instance.PastrySlotController.GetHistory(unit);
        foreach (ItemFood itemFood in _eatenPastry)
        {
            // Create a visual representation in the Pastry Slots.
        }
    }
}
