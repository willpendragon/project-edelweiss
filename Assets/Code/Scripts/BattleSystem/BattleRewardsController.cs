using UnityEngine;

public class BattleRewardsController : MonoBehaviour
{
    public float coinsRewardPool;
    public float experienceRewardPool;
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
}
