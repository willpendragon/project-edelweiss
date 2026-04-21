using UnityEngine;

[CreateAssetMenu(fileName = "New Achievement", menuName = "Achievement System/Kill Based Achievement")]
public class KillBasedAchievement : Achievement
{
    public int requiredKills;
    
    [Tooltip("The UnitTemplate of the enemy that must be hunted.")]
    public UnitTemplate targetEnemy;

    public override bool AchievementIsUnlocked()
    {
        Debug.Assert(spawnableDeity != null);

        // Fetch the amount of times the specific enemy was killed.
        int kills = 0;
        
        if (targetEnemy != null && SaveStateManager.saveData.killsByEnemyName != null)
        {
            SaveStateManager.saveData.killsByEnemyName.TryGetValue(targetEnemy.unitName, out kills);
        }
        else
        {
            // Fallback to total kills if no specific target is set (optional backwards compatibility)
            kills = SaveStateManager.saveData.enemiesKilled;
        }

        bool unlocked = kills >= requiredKills;
        
        if (spawnableDeity.TryGetComponent<Deity>(out Deity deity))
        {
            if (SaveStateManager.saveData.unitsLinkedToDeities.ContainsValue(deity.Id))
            {
                return false;
            }
        }
        
        return unlocked;
    }

    // Override the base class description to inject our custom values
    public override string GetDescription()
    {
        string formattedDescription = achievementDescription;

        if (targetEnemy != null)
        {
            formattedDescription = formattedDescription.Replace("{Target Enemy}", targetEnemy.unitName);
        }

        // Also replacing {Required Kills} as a bonus!
        formattedDescription = formattedDescription.Replace("{Required Kills}", requiredKills.ToString());

        return formattedDescription;
    }
}