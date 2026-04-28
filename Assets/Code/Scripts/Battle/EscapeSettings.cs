using UnityEngine;

[CreateAssetMenu(fileName = "EscapeSettings", menuName = "Settings/EscapeSettings")]
public class EscapeSettings : ScriptableObject
{
    [Header("Escape Configuration")]
    [Range(0f, 100f)]
    [Tooltip("The percentage chance (0-100) for a successful escape.")]
    public float escapeProbability = 50f;
    
    [Tooltip("Time in seconds the player has to cancel the escape.")]
    public float gracePeriod = 1.5f;
}
