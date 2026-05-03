using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CafeUnitHelper : MonoBehaviour
{
    public Unit unit;
    public int linkedUnitIndex;
    [SerializeField] private Animator _animator;
    void Start()
    {
        unit = GameManager.Instance.playerPartyMembersInstances[linkedUnitIndex];
        _animator = unit.gameObject.GetComponentInChildren<Animator>();
        GetComponent<Animator>().runtimeAnimatorController = _animator.runtimeAnimatorController;
    }
}
