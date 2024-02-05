using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleRewardsController : MonoBehaviour
{
    public float coinsRewardPool;
    public float experienceRewardPool;
    public BattleManager battleManager;

    public void OnEnable()
    {
        EnemyAgent.OnCoinsReward += AddCoinsRewardToCoinsRewardPool;
        EnemyAgent.OnExperienceReward += AddExperienceRewardToExperienceRewardPool;
        //BattleManager.OnBattleEnd += ApplyRewardsToPlayer;
    }
    public void OnDisable()
    {
        EnemyAgent.OnCoinsReward -= AddCoinsRewardToCoinsRewardPool;
        EnemyAgent.OnExperienceReward -= AddExperienceRewardToExperienceRewardPool;
        //BattleManager.OnBattleEnd -= ApplyRewardsToPlayer;
    }
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
        //battleManager.player.playerExperiencePoints += experienceRewardPool;
        //battleManager.player.coins += coinsRewardPool;
        GetComponent<Unit>().unitExperiencePoints += experienceRewardPool;
        GetComponent<Unit>().unitCoins += coinsRewardPool;
    }
}
