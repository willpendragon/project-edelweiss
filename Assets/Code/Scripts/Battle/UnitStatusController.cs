using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum UnitStatus
{
    stun,
    basic,
    Faithless
}

public class UnitStatusController : MonoBehaviour
{
    public UnityEvent UnitStun;
    public UnitStatus unitCurrentStatus;

    public void Start()
    {
        unitCurrentStatus = UnitStatus.basic;
    }
}
