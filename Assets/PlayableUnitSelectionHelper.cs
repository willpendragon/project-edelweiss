using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayableUnitSelectionHelper : MonoBehaviour
{
    public delegate void PlayableUnitSelected(Unit playableUnit);
    public static event PlayableUnitSelected OnPlayableUnitSelected;

    [SerializeField] private Unit unit;

    public void SelectionEventWrapper()
    {
        OnPlayableUnitSelected(unit);
    }
}
