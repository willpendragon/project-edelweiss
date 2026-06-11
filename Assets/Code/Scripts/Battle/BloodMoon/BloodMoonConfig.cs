using UnityEngine;

[CreateAssetMenu(fileName = "BloodMoonConfig", menuName = "Battle/Blood Moon Config")]
public class BloodMoonConfig : ScriptableObject
{
    [Header("Blood Moon Settings")]
    [SerializeField] private int _daysBeforeBloodMoon = 4;
    [SerializeField] private int _bloodMoonDuration = 3; // How many days the blood moon lasts
    [SerializeField] private float _enemyAttackPowerMultiplier = 1.5f; // 50% stronger
    
    public int DaysBeforeBloodMoon => _daysBeforeBloodMoon;
    public int BloodMoonDuration => _bloodMoonDuration;
    public float EnemyAttackPowerMultiplier => _enemyAttackPowerMultiplier;
}