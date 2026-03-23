using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PrizeReleaseController : MonoBehaviour
{
    // This class should be centralized instead of living on each enemy.

    // This enum controls the visuals only of the Prize.
    public enum FieldPrizeType
    {
        Upgrade,
        Key
    }

    [SerializeField] private GameObject _fieldPrizeGO;
    //public GameObject keyVisuals;
    //public ItemFieldPrize fieldPrize; // This slot contains the reward SO.
    private System.Random random;
    //public TextMeshProUGUI prizeTypeText;
    //FieldPrizeType fieldPrizeType;
    //[SerializeField] int keyPrizeThreshold;

    // A list containing the different types of items
    [SerializeField] List<ItemFieldPrize> _upgradePrizes;
    [SerializeField] List<ItemFieldPrize> _keyPrizes;

    // Weights for different outcomes, exposed to the editor
    [SerializeField] private List<int> weights = new List<int> { 70, 30 }; // Default: 70% chance to not win, 30% chance to win
    void Start()
    {
        random = new System.Random();
    }
    public void UnlockFieldPrize(TileController fieldPrizeTile)
    {
        //if (!RollFieldPrizeChance())
        //    return;

        // Set the Prize Type based on roll (either Key or Upgeades).
        FieldPrizeType selectedPrizeType = RollPrizeType();
        // Set where the Prize will actually spawn on the Tile.
        Vector3 prizeSpawnPosition = fieldPrizeTile.gameObject.transform.position + Vector3.up;

        if (selectedPrizeType == FieldPrizeType.Key)
        {
            // Only 1 key present at the moment, demo-only logic.
            SpawnPrize(fieldPrizeTile, prizeSpawnPosition, _keyPrizes[0]);
        }
        else if (selectedPrizeType == FieldPrizeType.Upgrade)
        {
            ItemFieldPrize rolledItem = RollUpgrade(selectedPrizeType);
            SpawnPrize(fieldPrizeTile, prizeSpawnPosition, rolledItem);
        }
    }

    private ItemFieldPrize RollUpgrade(FieldPrizeType fieldPrizeType)
    {
        // Retrieve the Prize SO from Upgrades list.
        // Only 2 Upgrades present at the moment, demo-only logic.
        var rng = Random.Range(0, 2);
        return _upgradePrizes[rng];

        //if (fieldPrizeType == FieldPrizeType.Upgrade)
        //{
        //    return _upgradePrizes[0];
        //}
        //else if (fieldPrizeType == FieldPrizeType.Upgrade)
        //{
        //    return _upgradePrizes[1];
        //}
        //else
        //{
        //    return null;
        //}
    }

    public bool RollFieldPrizeChance()
    {
        if (weights == null || weights.Count < 2)
        {
            Debug.LogError("Invalid weights list! Ensure you set it in the Inspector.");
            return false;
        }

        int totalWeight = weights.Sum();
        int roll = Random.Range(0, totalWeight);  // Use Unity's random
        int cumulativeWeight = 0;

        Debug.Log($"Total Weight: {totalWeight}, Rolled: {roll}");

        for (int i = 0; i < weights.Count; i++)
        {
            cumulativeWeight += weights[i];
            Debug.Log($"Checking Weight Index {i}, Cumulative: {cumulativeWeight}");

            if (roll < cumulativeWeight)
            {
                Debug.Log($"Selected Index: {i}, Win: {i != 0}");
                return i != 0; // Only index 0 is a loss, all others are wins
            }
        }

        return false;
    }

    public FieldPrizeType RollPrizeType()
    {
        // Roll the chance for the type of prize
        int rng = Random.Range(1, 3);

        switch (rng)
        {
            case 1:
                return FieldPrizeType.Upgrade;
            case 2:
                Debug.Log("Rolled a Key");
                return FieldPrizeType.Key;
            default:
                return FieldPrizeType.Upgrade;
        }
    }
    private void SpawnPrize(TileController fieldPrizeTile, Vector3 prizeSpawnPosition, ItemFieldPrize rolledItem)
    {
        // Instantiate the Field Prize GameObject at the new position
        GameObject newFieldPrizeGO = Instantiate(_fieldPrizeGO, prizeSpawnPosition, Quaternion.identity);
        //newFieldPrizeGO.GetComponent<PrizeReleaseController>().fieldPrize = this.fieldPrize;
        newFieldPrizeGO.GetComponent<FieldPrizeController>().fieldPrizeTemplate = rolledItem;
        newFieldPrizeGO.GetComponentInChildren<MeshFilter>().sharedMesh = rolledItem.prizeGraphics.GetComponent<MeshFilter>().sharedMesh;

        // Set the local scale of the new GameObject

        newFieldPrizeGO.transform.localScale = new Vector3(1, 1, 1);

        // Connects it to the Tile
        fieldPrizeTile.tileCurrentFieldPrize = newFieldPrizeGO;

        FieldPrizeController fieldPrizeController = newFieldPrizeGO?.GetComponent<FieldPrizeController>();
        fieldPrizeController.SetupPrize();

    }

}