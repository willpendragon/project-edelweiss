using System.Collections.Generic;
using UnityEngine;

public class EnemySelection : MonoBehaviour
{
    public GameObject[] enemySelection;
    public GridManager gridManager;
    //public List<EnemyType> EnemyTypeIds;
    public List<Vector2> EnemyCoordinates;
    public EnemyPartyData enemyParty;
    public int levelNumber;

    public void SelectMapNode()
    {
        GameManager.Instance.GenerateEnemyPartyData(enemyParty); // Generate random enemy data.
    }
}