using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PastrySlotsPanelHelper : MonoBehaviour
{
    [SerializeField] private List<ItemFood> _eatenPastry;
    [SerializeField] private GameObject _pastrySlotIcon;
    [SerializeField] RectTransform _pastrySlotsGrid;
    public void UpdatePastrySlots(Unit unit)
    {
        _eatenPastry.Clear();
        _eatenPastry = CafeMenuUIController.Instance.PastrySlotController.GetHistory(unit);
        foreach (ItemFood itemFood in _eatenPastry)
        {
            // Instantiate a GameObject on the Panel with the eaten pastry.
            GameObject newPastryIcon = Instantiate(_pastrySlotIcon, _pastrySlotsGrid);
            newPastryIcon.GetComponent<Image>().sprite = itemFood.foodIcon;

            // Add Unit icon to the Panel
        }
    }
}
