using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PastrySlotUIController : MonoBehaviour
{
    [SerializeField] public List<Unit> Units;
    [SerializeField] private Dictionary<Unit, List<ItemFood>> foodHistory = new();

    [SerializeField] private GameObject _pastrySlotsPanelPrefab;
    [SerializeField] private RectTransform _currentPastrySlotsContainer;
    private GameObject _newPastrySlotsPanel;


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

    public void CreatePastrySlotsPanel(Unit fedUnit, ItemFood foodItem)
    {
        // Add the food to the list of eaten Pastry for the corresponding character.
        TrackEatenFood(fedUnit, foodItem);
        // Update the Pastry Slots for the corresponding Character.
        _newPastrySlotsPanel = Instantiate(_pastrySlotsPanelPrefab, _currentPastrySlotsContainer.transform);
        _newPastrySlotsPanel.GetComponent<PastrySlotsPanelHelper>().UpdatePastrySlots(fedUnit);
        // Add Pastry Slot to Pastry Slots list (visual).

        // Save the list of eaten Pastry.
        // (...)
    }

    public void DestroyExistingSlotsPanel()
    {
        if (_newPastrySlotsPanel == null)
            return;
        Destroy(_newPastrySlotsPanel);
    }
}
