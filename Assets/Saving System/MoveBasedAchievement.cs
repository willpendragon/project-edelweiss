
using UnityEngine;

[CreateAssetMenu(fileName = "New Achievement", menuName = "Achievement System/Move Based Achievement")]
public class MoveBasedAchievement : Achievement
{
    public int requiredUsedMoves;

    public override bool AchievementIsUnlocked()
    {
        Debug.Assert(spawnableDeity != null);

        bool unlocked = SaveStateManager.saveData.timesSingleTargetSpellWasUsed >= requiredUsedMoves;
        if (spawnableDeity.TryGetComponent<Deity>(out Deity deity))
        {
            if (SaveStateManager.saveData.unitsLinkedToDeities.ContainsValue(deity.Id))
            {
                return false;
            }
        }
        return unlocked;
    }
}