using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PrizeReleaseController : MonoBehaviour
{
    public enum FieldPrizeType
    {
        Upgrade,
        Key
    }

    [Header("Prefabs & Data")]
    [SerializeField] private GameObject _fieldPrizeGO;
    [SerializeField] private List<ItemFieldPrize> _upgradePrizes;
    [SerializeField] private List<ItemFieldPrize> _keyPrizes;

    // Chest Key specific drops are loaded locally during DropSpecificPrize to keep enemy instances clean

    [Header("Drop Chances")]
    [Tooltip("Index 0 is Loss chance. Index 1+ is Win chance.")]
    [SerializeField] private List<int> weights = new List<int> { 70, 30 };

    // Expose FieldPrizeGO internally for dynamic chest loading
    public GameObject FieldPrizePrefab
    {
        get => _fieldPrizeGO;
        set => _fieldPrizeGO = value;
    }

    public void UnlockFieldPrize(TileController fieldPrizeTile)
    {
        // 1. THE GUARD CLAUSE: Prevent double spawning!
        // If the tile already holds a prize, abort the sequence immediately.
        if (fieldPrizeTile.tileCurrentFieldPrize != null)
        {
            Debug.Log($"Tile {fieldPrizeTile.name} already has a prize. Skipping spawn.");
            return;
        }

        // 2. Roll for success
        if (!RollFieldPrizeChance())
            return;

        // 3. Determine Prize Type and Location
        FieldPrizeType selectedPrizeType = RollPrizeType();
        Vector3 prizeSpawnPosition = fieldPrizeTile.gameObject.transform.position + Vector3.up;

        // 4. Spawn the appropriate prize
        if (selectedPrizeType == FieldPrizeType.Key)
        {
            if (_keyPrizes.Count > 0)
                SpawnPrize(fieldPrizeTile, prizeSpawnPosition, _keyPrizes[0]);
        }
        else if (selectedPrizeType == FieldPrizeType.Upgrade)
        {
            ItemFieldPrize rolledItem = RollUpgrade();
            if (rolledItem != null)
                SpawnPrize(fieldPrizeTile, prizeSpawnPosition, rolledItem);
        }
    }

    private ItemFieldPrize RollUpgrade()
    {
        // Failsafe: Ensure the list isn't empty before rolling
        if (_upgradePrizes == null || _upgradePrizes.Count == 0)
        {
            Debug.LogWarning("Upgrade Prizes list is empty!");
            return null;
        }

        // Make it dynamic: Rolls between 0 and whatever the list size is.
        int rng = Random.Range(0, _upgradePrizes.Count);
        return _upgradePrizes[rng];
    }

    public bool RollFieldPrizeChance()
    {
        if (weights == null || weights.Count < 2)
        {
            Debug.LogError("Invalid weights list! Ensure you set it in the Inspector.");
            return false;
        }

        int totalWeight = weights.Sum();
        int roll = Random.Range(0, totalWeight);
        int cumulativeWeight = 0;

        for (int i = 0; i < weights.Count; i++)
        {
            cumulativeWeight += weights[i];

            if (roll < cumulativeWeight)
            {
                return i != 0; // Index 0 is a loss, anything else is a win
            }
        }

        return false;
    }

    public FieldPrizeType RollPrizeType()
    {
        // Simplify the roll: Random.Range with integers is (inclusive, exclusive).
        // This gives a 50/50 chance for 0 or 1.
        int rng = Random.Range(0, 2);

        if (rng == 0) return FieldPrizeType.Upgrade;
        else return FieldPrizeType.Key;
    }

    private void SpawnPrize(TileController fieldPrizeTile, Vector3 prizeSpawnPosition, ItemFieldPrize rolledItem)
    {
        // Instantiate and set scale
        GameObject newFieldPrizeGO = Instantiate(_fieldPrizeGO, prizeSpawnPosition, Quaternion.identity);
        newFieldPrizeGO.transform.localScale = Vector3.one;

        // Link the item to the tile
        fieldPrizeTile.tileCurrentFieldPrize = newFieldPrizeGO;
        fieldPrizeTile.currentSingleTileStatus = SingleTileStatus.selectedPlayerUnitOccupiedTile;

        // Apply visual meshes
        MeshFilter targetMeshFilter = newFieldPrizeGO.GetComponentInChildren<MeshFilter>();
        MeshFilter sourceMeshFilter = rolledItem.prizeGraphics.GetComponent<MeshFilter>();

        if (targetMeshFilter != null && sourceMeshFilter != null)
        {
            targetMeshFilter.sharedMesh = sourceMeshFilter.sharedMesh;
        }

        // Setup the controller data
        FieldPrizeController fieldPrizeController = newFieldPrizeGO.GetComponent<FieldPrizeController>();
        if (fieldPrizeController != null)
        {
            fieldPrizeController.fieldPrizeTemplate = rolledItem;
            fieldPrizeController.SetupPrize();
        }
    }

    public void DropSpecificPrize(TileController fieldPrizeTile, ChestTemplate.ChestPrizeType prizeType, ItemFieldPrize specificKeyLoadedSO)
    {
        if (fieldPrizeTile.tileCurrentFieldPrize != null)
        {
            Debug.Log($"Tile {fieldPrizeTile.name} already has a prize. Skipping spawn.");
            return; // Prevent stacking
        }

        // Guaranteed Failsafe: Dynamically load the base prefab if missing (e.g., dynamically built Chests)
        if (_fieldPrizeGO == null)
        {
            _fieldPrizeGO = Resources.Load<GameObject>("GenericKeyPrize");
            if (_fieldPrizeGO == null)
            {
                Debug.LogError("[PrizeReleaseController] Critical Error: 'GenericKeyPrize' prefab not found in Resources folder!");
                return;
            }
        }

        Vector3 prizeSpawnPosition = fieldPrizeTile.gameObject.transform.position + Vector3.up;

        if (specificKeyLoadedSO != null)
        {
            SpawnPrize(fieldPrizeTile, prizeSpawnPosition, specificKeyLoadedSO);
        }
        else if (prizeType == ChestTemplate.ChestPrizeType.SimpleKey && _keyPrizes.Count > 0)
        {
            SpawnPrize(fieldPrizeTile, prizeSpawnPosition, _keyPrizes[0]); // Fallback generic key
        }
        else
        {
            Debug.LogWarning($"[PrizeReleaseController] Missing Loaded SO Reference for specific {prizeType} chest drop!");
        }
    }
}