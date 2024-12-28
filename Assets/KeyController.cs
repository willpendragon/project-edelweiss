using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class KeyController : MonoBehaviour
{
    [SerializeField] GameObject key = null;

    public bool ValidateKey()
    {
        if (key == null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public BattleTypeController.BattleType UnlockLevelLogic()
    {
        if (ValidateKey())
        {
            return BattleTypeController.BattleType.PuzzleBattle;
        }
        else
        {
            return BattleTypeController.BattleType.BattleWithDeity;
        }
    }
}
