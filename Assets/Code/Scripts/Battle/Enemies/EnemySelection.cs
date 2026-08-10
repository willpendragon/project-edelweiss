using System.Collections.Generic;
using UnityEngine;

public class EnemySelection : MonoBehaviour
{
    // The name is misleading since this now also handles the Map data.
    // 09082026 Aaand it misled me. :S I make a wow to change this class name, lest always fall in the same pit...

    public GameObject[] enemySelection;
    //public GridManager gridManager;
    //public List<EnemyType> EnemyTypeIds;
    public List<Vector2> EnemyCoordinates;
    public EnemyPartyData enemyParty;
    public int levelNumber;
    public MapData mapData;
    public string conversationTitle;


    public void SelectMapNode()
    {
        // Sets the current Map on the GridManager.
        GridManager.Instance.currentMapData = mapData;
        // Generate the actual map retrieved from the node.
        GridManager.Instance.GenerateGridMapFromData(mapData);
        // Sets the current Map on the GameManager (to ensure the choice stays persistent when entering Battle).
        GameManager.Instance.CurrentMap = mapData;
        GameManager.Instance.EnemyPartyManager.GenerateEnemyPartyData(enemyParty); // Generate random enemy data.
        // Injects the convo number into GameManager, so it can be retrieved at the start of the fight.
        GameManager.Instance.currentConversationTitle = conversationTitle;
    }
}