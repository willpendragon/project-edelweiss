using UnityEngine;


public abstract class Achievement : ScriptableObject
{
    public string achievementName;
    public string achievementDescription;
    public GameObject spawnableDeity;

    public abstract bool AchievementIsUnlocked();
}
