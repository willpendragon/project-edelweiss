using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class FieldPrizeController : MonoBehaviour
{
    public GameObject fieldPrizeVisuals;
    public ItemFieldPrize fieldPrize;
    private System.Random random;

    // Weights for different outcomes, exposed to the editor
    [SerializeField]
    private List<int> weights = new List<int> { 70, 30 }; // Default: 70% chance to not win, 30% chance to win

    void Start()
    {
        random = new System.Random();
    }

    public void SpawnFieldPrize(TileController fieldPrizeTile)
    {
        if (RollFieldPrizeChance())
        {
            // Spawns the Field Prize GameObject
            // Calculate the new spawn position
            Vector3 prizeSpawnPosition = fieldPrizeTile.gameObject.transform.position + new Vector3(0, 1, 0);

            // Instantiate the Field Prize GameObject at the new position
            GameObject newFieldPrizeGO = Instantiate(fieldPrizeVisuals, prizeSpawnPosition, Quaternion.identity);
            newFieldPrizeGO.GetComponent<FieldPrizeController>().fieldPrize = this.fieldPrize;

            // Set the local scale of the new GameObject
            newFieldPrizeGO.transform.localScale = new Vector3(1, 1, 1);
            // Connects it to the Tile
            fieldPrizeTile.tileCurrentFieldPrize = newFieldPrizeGO;
            Debug.Log("Spawned Field Prize" + newFieldPrizeGO.GetComponent<FieldPrizeController>().fieldPrize.itemFieldPrizeType);
        }
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
}