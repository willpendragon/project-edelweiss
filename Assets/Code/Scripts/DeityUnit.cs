using UnityEngine;

[CreateAssetMenu(fileName = "New DeityUnit", menuName = "Deity/Unit")]
public class DeityUnit : ScriptableObject
{
    public float deityHp;
    public float deityAttackPower;
    public string deityName;
    // Add other stats here.
}