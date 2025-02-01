using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class KeyController : MonoBehaviour
{
    public BattleTypeController.BattleType UnlockLevelLogic()
    {
        if (ValidateKey())
        {
            return BattleTypeController.BattleType.PuzzleBattle;
        }
        else
        {
            return BattleTypeController.BattleType.RegularBattle;
        }
    }
    public bool ValidateKey()
    {
        GameStatsManager gameStatsManager = GameObject.FindGameObjectWithTag("GameStatsManager").GetComponent<GameStatsManager>();
        if (gameStatsManager.unlockedPuzzleKeys >= 1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
