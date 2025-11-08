using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CafeUnitHelper : MonoBehaviour
{
    public Unit unit;
    public int linkedUnitIndex;
    void Start()
    {
        unit = GameManager.Instance.playerPartyMembersInstances[linkedUnitIndex];
    }
}
