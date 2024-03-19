using UnityEngine;

[CreateAssetMenu(fileName = "New Achievement", menuName = "Achievement System/Achievement")]

public class Achievement : ScriptableObject
{
    public enum AchievementType
    {
        killedEnemies
    }

    public string achievementName;
    public string description;
    public int requirement;
    public AchievementType achievementType;
    public GameObject spawnableDeity;
}