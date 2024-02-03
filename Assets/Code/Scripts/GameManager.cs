using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public NodeController currentNode;
    public static GameManager _instance;
    public GameObject[] currentEnemySelection;
    public EnemySelection currentEnemySelectionComponent;
    public List<EnemyType> currentEnemySelectionIds;
    public List<Vector2> currentEnemySelectionCoords;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
            return;
        }
        Destroy(this.gameObject);
    }

    public void OnEnable()
    {
        //EnemySelection.OnSelectedMapNodeWithEnemies += DefineEnemySelection;
    }
    public void OnDisable()
    {
        //EnemySelection.OnSelectedMapNodeWithEnemies -= DefineEnemySelection;
    }
    public void MarkCurrentNodeAsCompleted()

    {
        currentNode.nodeCompleted = true;
    }
    public void DefineEnemySelection(GameObject[] enemySelection)
    {
        //currentEnemySelection = enemySelection;
    }
    public void UpdateEnemyData(Level level)
    {
        currentEnemySelectionIds.Clear();
        currentEnemySelectionCoords.Clear();

        for (int i = 0; i < level.EnemyTypeIds.Count; i++)
        {
            currentEnemySelectionIds.Add(level.EnemyTypeIds[i]);
            currentEnemySelectionCoords.Add(level.UnitCoordinates[i]);
        }
    }
}
