
using UnityEngine;

[CreateAssetMenu(fileName = "New Achievement", menuName = "Achievement System/Kill Based Achievement")]
public class KillBasedAchievement : Achievement
{
    public int requiredKills;

    public override bool AchievementIsUnlocked()
    {
        Debug.Assert(spawnableDeity != null);

        bool unlocked = SaveStateManager.saveData.enemiesKilled >= requiredKills;
        if(spawnableDeity.TryGetComponent<Deity>(out Deity deity))
        {
            if (SaveStateManager.saveData.unitsLinkedToDeities.ContainsValue(deity.Id))
            {
                return false;
            }
        }
        
        return unlocked;
    }
}