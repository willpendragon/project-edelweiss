using UnityEngine;

public class ChestUnit : Unit
{
    public override void CheckUnitHealthStatus()
    {
        if (unitHealthPoints > 0)
        {
            Debug.Log("Chest structure is still intact.");
        }
        else if (unitHealthPoints <= 0)
        {
            var meshRenderer = GetComponentInChildren<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.material.color = Color.black; // Simple visual death feedback
            }

            currentUnitLifeCondition = UnitLifeCondition.unitDead;
            CheckEnemyDefeat();

            if (ownedTile != null && ownedTile.tileShaderController != null)
            {
                ownedTile.tileShaderController.SetTileToMoveRangeColor();
                ownedTile.tileShaderController.SetTileGlowIntensity(1f);
            }
        }
    }

    protected override void CheckEnemyDefeat()
    {
        // Instead of typical enemy rewards, we process the specific chest loot
        if (unitTemplate is ChestTemplate chestConfig)
        {
            Debug.Log($"[CHEST] Destroyed! Dropping specific key: {chestConfig.chestPrizeType}");

            switch (chestConfig.chestPrizeType)
            {
                case ChestTemplate.ChestPrizeType.SimpleKey:
                    // Drop Simple Key Logic
                    break;
                case ChestTemplate.ChestPrizeType.MinibossKey:
                    // Drop Miniboss Key Logic
                    break;
                case ChestTemplate.ChestPrizeType.BossKey:
                    // Drop Boss Key Logic
                    break;
            }
        }
        else
        {
            Debug.Log("[CHEST] Destroyed! Dropping default SimpleKey (No ChestTemplate assigned).");
        }

        // Free up the tile so units can step on it
        if (ownedTile != null)
        {
            ownedTile.currentSingleTileCondition = SingleTileCondition.free;
            ownedTile.detectedUnit = null;
        }

        // Turn off the physical chest prototype
        gameObject.SetActive(false);
    }
}