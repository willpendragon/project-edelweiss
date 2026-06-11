using UnityEngine;

[CreateAssetMenu(fileName = "New DeityUnit", menuName = "Deity/Deity Stats")]
public class DeityUnit : ScriptableObject
{
    // Currently the Unit class is being used to handle Deities stats as well.

    public float deityHp;
    public float deityAttackPower;
    public string deityName;
    // Add other stats here.
}