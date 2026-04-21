using UnityEngine;

public abstract class Achievement : ScriptableObject
{
    public string achievementName;
    [TextArea(3, 5)] // Optional: makes it easier to write longer descriptions in the inspector
    [Tooltip("Description of the achievement. Use {Target Enemy} to display the name of the enemy the Player has to hunt.")]
    public string achievementDescription;
    public GameObject spawnableDeity;

    public abstract bool AchievementIsUnlocked();

    // The virtual method allows inherited classes like KillBasedAchievement to inject their own text
    public virtual string GetDescription()
    {
        return achievementDescription;
    }
}
