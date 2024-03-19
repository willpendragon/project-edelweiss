using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeitySpawner : MonoBehaviour
{
    [SerializeField] GameObject[] spawnableDeities;
    [SerializeField] Transform deitySpawnPosition;
    [SerializeField] DeityAchievementsController deityAchievementsController;
    [SerializeField] BattleManager battleManager;

    public Deity currentUnboundDeity;
    // Start is called before the first frame update
    public GameObject deityHealthBar;
    void Start()
    {
        if (battleManager.currentBattleType == BattleType.regularBattle)
        {
            var deityRoll = Random.Range(0, 7);
            if (deityRoll <= 6 && deityRoll >= 3)
            {
                DeitySelector();
                Debug.Log("Rolled Deity arrival on battlefield");
            }
        }
    }
    public void DeitySelector()
    {
        Debug.Log("Rolling which Deity will appear");
        int deityIndex = Random.Range(0, spawnableDeities.Length);
        GameObject spawningDeity = spawnableDeities[deityIndex];
        Instantiate(spawningDeity, deitySpawnPosition);
        GameObject.FindGameObjectWithTag("BattleManager").GetComponent<BattleManager>().deity = spawningDeity.GetComponent<Deity>();
    }
    void CreateDeityHealthBar(GameObject spawnedUnboundDeity)
    {
        Deity currentSpawnedUnboundDeity = spawnedUnboundDeity.GetComponent<Deity>();

        GameObject battleInterfaceCanvasGO = GameObject.FindGameObjectWithTag("BattleInterfaceCanvas");
        GameObject deityHealthBarInstance = Instantiate(deityHealthBar, battleInterfaceCanvasGO.transform);
        Slider deityHPSlider = deityHealthBarInstance.GetComponentInChildren<Slider>();
        currentSpawnedUnboundDeity.deityHealthBar = deityHealthBarInstance;

        Unit currentSpawnedUnboundDeityUnit = currentSpawnedUnboundDeity.GetComponentInChildren<Unit>();

        deityHPSlider.maxValue = currentSpawnedUnboundDeityUnit.unitMaxHealthPoints;
        deityHPSlider.value = currentSpawnedUnboundDeityUnit.unitHealthPoints;
        deityHPSlider.GetComponentInChildren<TextMeshProUGUI>().text = currentSpawnedUnboundDeityUnit.unitMaxHealthPoints.ToString();

        Debug.Log("Spawning Deity Health Bar");
    }
    public void UpdateDeityHealthBar()
    {

    }

    public void InitiateBattleWithDeity(GameObject unlockedDeity)
    {
        //Unlocks Anguana as an Unbound Entity
        //0 is a magic number, remove it later
        Debug.Log("Unbound Anguana Unlocked");
        unlockedDeity.GetComponent<Unit>().startingXCoordinate = 5;
        unlockedDeity.GetComponent<Unit>().startingYCoordinate = 9;
        GameObject unboundDeity = Instantiate(unlockedDeity);
        Debug.Log("Moving Unbound Deity to Starting Position");
        if (unboundDeity != null)
        {
            Debug.Log("Start of Summon Deity on Battlefield");
            unboundDeity.GetComponent<Unit>().MoveUnit(5, 9);
            //Beware, Magic Numbers
            TileController deitySpawningTile = GridManager.Instance.GetTileControllerInstance(5, 9);

            unboundDeity.GetComponent<Unit>().ownedTile = deitySpawningTile;

            //Deity occupies multiple tiles
            foreach (var deitySpawningZoneTile in GameObject.FindGameObjectWithTag("GridMovementController").GetComponent<GridMovementController>().GetMultipleTiles(deitySpawningTile))
            {
                deitySpawningZoneTile.currentSingleTileCondition = SingleTileCondition.occupiedByDeity;
                deitySpawningZoneTile.GetComponentInChildren<MeshRenderer>().material.color = Color.magenta;
                deitySpawningZoneTile.detectedUnit = unboundDeity;
                currentUnboundDeity = unboundDeity.GetComponent<Deity>();
                Debug.Log("Deity occupies Tile");
            }

            foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
            {
                Destroy(enemy);
            }
            unboundDeity.gameObject.tag = "Enemy";
            CreateDeityHealthBar(unboundDeity);
        }
    }
}
