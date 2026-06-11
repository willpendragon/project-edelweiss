using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RecruitSlotUI : MonoBehaviour, IPointerClickHandler
{
    [HideInInspector] public bool isActiveSlot;
    [HideInInspector] public int slotIndex; // Only used if isActiveSlot is true
    [HideInInspector] public Unit assignedUnit;
    [SerializeField] public Image portrait;

    public void OnPointerClick(PointerEventData eventData)
    {
        // Right Click: Removes from party
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (isActiveSlot && assignedUnit != null)
            {
                RecruitManager.Instance.RemoveFromActiveParty(slotIndex);
            }
        }
        // Left Click: Adds to party
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (!isActiveSlot && assignedUnit != null)
            {
                RecruitManager.Instance.AddToActiveParty(assignedUnit);
            }
        }
    }
}