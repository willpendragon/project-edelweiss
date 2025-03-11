using UnityEngine;

public class FaithController : MonoBehaviour
{
    [SerializeField] PlayerPartyController playerPartyController;
    [SerializeField] int faithPointReductionRate = 10;
    private System.Random localRandom = new System.Random();
    private void Start()
    {
        if (playerPartyController != null)
        {
            foreach (GameObject playerUnitGameObject in playerPartyController.playerUnitsOnBattlefield)
            {
                Unit playerUnit = playerUnitGameObject.GetComponent<Unit>();
                int faithPointsReduction = CalculateReduction(playerUnit.unitFaithPoints);
                if (playerUnit.unitFaithPoints > 0)
                {
                    playerUnit.unitFaithPoints -= faithPointsReduction;
                    Debug.Log(faithPointsReduction);
                }
            }
        }
    }
    int CalculateReduction(int playerUnitFaithPoints)
    {
        int randomRate = localRandom.Next(10, 30);
        float reduction = (playerUnitFaithPoints * randomRate) / 100f;
        return Mathf.Max(1, Mathf.RoundToInt(reduction));
    }

    public void CheckFaithPoints()
    {
        foreach (GameObject playerUnitGameObject in playerPartyController.playerUnitsOnBattlefield)
        {
            Unit playerUnit = playerUnitGameObject.GetComponent<Unit>();
            if (playerUnit.unitFaithPoints <= 0)
            {
                playerUnit.unitStatusController.unitCurrentStatus = UnitStatus.Faithless;
                Debug.Log("This Unit is now Faithless");
            }
        }
    }
}