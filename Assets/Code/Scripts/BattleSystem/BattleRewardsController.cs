using System.Collections;
using UnityEngine;
using static TurnController;

public class BattleRewardsController : MonoBehaviour
{
    public float coinsRewardPool;
    public float experienceRewardPool;
    public int multiKillCounter;
    public void AddCoinsRewardToCoinsRewardPool(float coinsRewardToAdd)
    {
        coinsRewardPool += coinsRewardToAdd;
        Debug.Log("Adding Coins Reward");
    }
    public void AddExperienceRewardToExperienceRewardPool(float experienceRewardToAdd)
    {
        experienceRewardPool += experienceRewardToAdd;
        Debug.Log("Adding Experience Reward");
    }
    public void ApplyRewardsToThisUnit()
    {
        GetComponent<Unit>().unitExperiencePoints += experienceRewardPool;
        GetComponent<Unit>().unitCoins += coinsRewardPool;
    }
    public void ApplyPartyRewardsAndSave(float warFunds)
    {
        // Saves each Player's Health Points, Coins and Experience Rewards.
        GameStatsManager gameStatsManager = GameObject.FindGameObjectWithTag(Tags.GAME_STATS_MANAGER).GetComponent<GameStatsManager>();

        gameStatsManager.captureCrystalsCount += BattleManager.Instance.captureCrystalsRewardPool;
        gameStatsManager.SaveEnemiesKilled();
        gameStatsManager.SaveCharacterData();
        gameStatsManager.SaveWarFunds(warFunds);
        gameStatsManager.SaveUsedSingleTargetSpells();
        gameStatsManager.SaveCaptureCrystalsCount();
        Debug.Log("Saving Character Stats Data");
    }
    public void IncreaseMultiKillCounter(int newKill)
    {
        multiKillCounter += newKill;
    }
    public int CalculateMultiKillCounter()
    {
        switch (multiKillCounter)
        {
            case 1:
                return 1;
            case 2:
                return 2;
            case 3:
                return 3;
            default:
                return 1;
        }
    }
    public void resetMultiKillCounter()
    {
        StartCoroutine("ExecuteReset");
    }
    IEnumerator ExecuteReset()
    {
        yield return new WaitForSeconds(1);
        multiKillCounter = 0;
    }
}
