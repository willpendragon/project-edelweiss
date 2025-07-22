using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSelectionHelper : MonoBehaviour
{
    public delegate void UnitSelected(Unit Unit);
    public static event UnitSelected OnUnitSelected;

    [SerializeField] private Unit unit;

    public void SelectionEventWrapper()
    {
        OnUnitSelected(unit);
    }
}
