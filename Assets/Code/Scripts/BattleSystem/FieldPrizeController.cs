using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class FieldPrizeController : MonoBehaviour
{
    public enum FieldPrizeType
    {
        Upgrade,
        Key
    }
    public GameObject fieldPrizeVisuals;
    public GameObject keyVisuals;
    public ItemFieldPrize fieldPrize;
    private System.Random random;
    public TextMeshProUGUI prizeTypeText;
    FieldPrizeType fieldPrizeType;
    [SerializeField] int keyPrizeThreshold;

    // Weights for different outcomes, exposed to the editor
    [SerializeField]
    private List<int> weights = new List<int> { 70, 30 }; // Default: 70% chance to not win, 30% chance to win
    void Start()
    {
        random = new System.Random();
    }
    public void UnlockFieldPrize(TileController fieldPrizeTile)
    {
        if (!RollFieldPrizeChance())
            return;

        FieldPrizeType selectedPrizeType = SelectPrizeType();
        Vector3 prizeSpawnPosition = fieldPrizeTile.gameObject.transform.position + Vector3.up;

        GameObject prizeVisualPrefab = selectedPrizeType == FieldPrizeType.Upgrade ? fieldPrizeVisuals : keyVisuals;
        SpawnPrize(fieldPrizeTile, prizeSpawnPosition, prizeVisualPrefab);

        Debug.Log($"Spawned {selectedPrizeType}.");
    }
    public bool RollFieldPrizeChance()
    {
        int totalWeight = weights.Sum();
        int roll = random.Next(totalWeight);
        int cumulativeWeight = 0;

        for (int i = 0; i < weights.Count; i++)
        {
            cumulativeWeight += weights[i];
            if (roll < cumulativeWeight)
            {
                return i == 1; // Assuming index 1 is the winning outcome
            }
        }

        return false;
    }
    public FieldPrizeType SelectPrizeType()
    {
        if (ComboController.Instance.comboCounter >= keyPrizeThreshold)
        {
            return
            fieldPrizeType = FieldPrizeType.Key;
        }
        else
        {
            return
            fieldPrizeType = FieldPrizeType.Upgrade;
        }
    }
    private void SpawnPrize(TileController fieldPrizeTile, Vector3 prizeSpawnPosition, GameObject spawnablePrizeVisuals)
    {
        // Instantiate the Field Prize GameObject at the new position
        GameObject newFieldPrizeGO = Instantiate(spawnablePrizeVisuals, prizeSpawnPosition, Quaternion.identity);
        newFieldPrizeGO.GetComponent<FieldPrizeController>().fieldPrize = this.fieldPrize;

        // Set the local scale of the new GameObject

        newFieldPrizeGO.transform.localScale = new Vector3(1, 1, 1);

        // Connects it to the Tile
        fieldPrizeTile.tileCurrentFieldPrize = newFieldPrizeGO;

        FieldPrizeController newFieldPrize = newFieldPrizeGO?.GetComponent<FieldPrizeController>();

        ApplyTextLabelToPrize(newFieldPrize);

        Debug.Log("Spawned Field Prize" + newFieldPrizeGO.GetComponent<FieldPrizeController>().fieldPrize.itemFieldPrizeType);
    }
    private void ApplyTextLabelToPrize(FieldPrizeController newFieldPrize)
    {
        switch (newFieldPrize.fieldPrize?.itemFieldPrizeType)
        {
            case ItemFieldPrizeType.attackPowerUp:
                newFieldPrize.prizeTypeText.text = "ATK+";
                break;
            case ItemFieldPrizeType.magicPowerUp:
                newFieldPrize.prizeTypeText.text = "MAGI+";
                break;
            default:
                break;
        }
    }
}