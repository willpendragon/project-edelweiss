using UnityEngine;

public class PrizeCollectionHelper : MonoBehaviour
{
    public delegate void PrizeCollected(Color color);
    public static event PrizeCollected OnPrizeCollected;

    public delegate void UpgradeObtained(string message);
    public static event UpgradeObtained OnUpgradeObtained;

    // This class controls the logic of a Unit grabbing a Prize on the Battlefield.
    public void CheckFieldPrizes(TileController destinationTile, Unit activePlayerUnit)
    {
        if (destinationTile != null && destinationTile.tileCurrentFieldPrize != null)
        {
            FieldPrizeController fieldPrizeController = destinationTile.tileCurrentFieldPrize.GetComponent<FieldPrizeController>();
            if (fieldPrizeController != null && fieldPrizeController.ItemFieldPrizeType == ItemFieldPrizeType.attackPowerUp) // Using the public read-only variable from controller.
            {
                activePlayerUnit.unitAttackPower += fieldPrizeController.PowerUpAmount;
                OnPrizeCollected(Color.red); // Display Prize Collected Feedback
                DisplayUpgradeNotification($"{activePlayerUnit.unitTemplate.unitName}'s Attack Power increased.");
            }
            else if (fieldPrizeController != null && fieldPrizeController.ItemFieldPrizeType == ItemFieldPrizeType.magicPowerUp)
            {
                activePlayerUnit.unitMagicPower += fieldPrizeController.PowerUpAmount;
                OnPrizeCollected(Color.magenta);
                DisplayUpgradeNotification($"{activePlayerUnit.unitTemplate.unitName}'s Magic Power increased.");
            }
            else if (fieldPrizeController != null && fieldPrizeController.ItemFieldPrizeType == ItemFieldPrizeType.PuzzleLevelKey)
            {
                // Logic for unlocking keys
                GameStatsManager gameStatsManager = GameObject.FindGameObjectWithTag("GameStatsManager").GetComponent<GameStatsManager>();
                gameStatsManager.unlockedPuzzleKeys += 1;
                gameStatsManager.SaveUnlockedKeys(gameStatsManager.unlockedPuzzleKeys);
                Debug.Log("Added Key to Game Stats Manager and saved to game state");
            }
            UpdateCombatValues();
            Destroy(fieldPrizeController.gameObject);
        }
    }
    private void UpdateCombatValues()
    {
        Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit")?.GetComponent<Unit>();
        if (activePlayerUnit != null)
        {
            activePlayerUnit.unitProfilePanel.GetComponent<UnitProfileController>().activeCharacterAttackPower.text = activePlayerUnit.unitAttackPower.ToString();
            activePlayerUnit.unitProfilePanel.GetComponent<UnitProfileController>().activeCharacterMagicPower.text = activePlayerUnit.unitMagicPower.ToString();
        }
    }

    private void DisplayUpgradeNotification(string message)
    {
        OnUpgradeObtained(message);
    }

}
