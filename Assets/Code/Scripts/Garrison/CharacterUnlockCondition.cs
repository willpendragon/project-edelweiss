using UnityEngine;

[CreateAssetMenu(fileName = "New Character Unlock", menuName = "Progression/Character Unlock Condition")]
public class CharacterUnlockCondition : ScriptableObject
{
    [Tooltip("The actual Unit prefab you want to unlock.")]
    public Unit characterToUnlock;

    [Header("Requirements")]
    [Tooltip("The exact Name or ID of the Deity the player must kill (as tracked in GameSaveData.killedDeities) to unlock this character.")]
    public string requiredDeityNameKilled;

    /// <summary>
    /// Evaluates if the condition to unlock this character has been met based on the player's save file.
    /// </summary>
    public bool IsConditionMet(GameSaveData saveData)
    {
        if (saveData == null || saveData.killedDeities == null) return false;

        // For the demo: Returns true only if the specified deity is registered as killed
        if (!string.IsNullOrEmpty(requiredDeityNameKilled))
        {
            if (saveData.killedDeities.ContainsKey(requiredDeityNameKilled) && saveData.killedDeities[requiredDeityNameKilled] == true)
            {
                return true;
            }
        }

        return false;
    }
}