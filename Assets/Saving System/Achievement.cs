using UnityEngine;


public abstract class Achievement : ScriptableObject
{
    public string achievementName;
    public string description;
    public GameObject spawnableDeity;

    public abstract bool AchievementIsUnlocked();
}
