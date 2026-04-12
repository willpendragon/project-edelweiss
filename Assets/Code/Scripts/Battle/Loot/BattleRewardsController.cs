using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TurnController;
public class BattleRewardsController : MonoBehaviour
{
    public float coinsRewardPool;
    public float experienceRewardPool;
    public int multiKillCounter;
    public List<Ingredient> ingredients;
    
    public bool acquiredMinibossKey;
    public bool acquiredBossKey;

    public void AddCoinsRewardToCoinsRewardPool(float coinsRewardToAdd)
    {
        coinsRewardPool += coinsRewardToAdd;
    }
    public void AddExperienceRewardToExperienceRewardPool(float experienceRewardToAdd)
    {
        experienceRewardPool += experienceRewardToAdd;
    }

    public void AddTemporaryLoot(Ingredient ingredient)
    {
        ingredients.Add(ingredient);
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

        // Saves the Looted Ingredients
        gameStatsManager.SaveIngredients();

        // Flush acquired story keys to Persistent Saver now that victory is guaranteed!
        if (acquiredMinibossKey) gameStatsManager.SaveMinibossKey(true);
        if (acquiredBossKey) gameStatsManager.SaveBossKey(true);
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
