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
        BattleManager.OnBattleEnd += ApplyRewardsToPlayer;
    }
    public void OnDisable()
    {
        EnemyAgent.OnCoinsReward -= AddCoinsRewardToCoinsRewardPool;
        EnemyAgent.OnExperienceReward -= AddExperienceRewardToExperienceRewardPool;
        BattleManager.OnBattleEnd -= ApplyRewardsToPlayer;
    }

    void AddCoinsRewardToCoinsRewardPool(float coinsRewardToAdd)
    {
        coinsRewardPool += coinsRewardToAdd;
        Debug.Log("Adding Coins Reward");
    }
    void AddExperienceRewardToExperienceRewardPool(float experienceRewardToAdd)
    {
        experienceRewardPool += experienceRewardToAdd;
        Debug.Log("Adding Experience Reward");
    }

    void ApplyRewardsToPlayer()
    {
        battleManager.player.playerExperiencePoints += experienceRewardPool;
        battleManager.player.coins += coinsRewardPool;
    }
}
