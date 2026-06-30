using UnityEngine;

[CreateAssetMenu(fileName = "ParryConfig", menuName = "Combat/Parry Configuration")]
public class ParryConfig : ScriptableObject
{
    [Tooltip("How long (in seconds) the player has to press the parry button.")]
    public float windowOfOpportunity = 0.5f;

    [Tooltip("The button used to parry.")]
    public KeyCode parryKey = KeyCode.E;
}