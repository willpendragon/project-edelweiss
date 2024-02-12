using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeitySpawner : MonoBehaviour
{
    [SerializeField] GameObject[] spawnableDeities;
    [SerializeField] Transform deitySpawnPosition;
    [SerializeField] DeityAchievementsController deityAchievementsController;
    [SerializeField] BattleManager battleManager;
    // Start is called before the first frame update
    void Start()
    {
        if (deityAchievementsController.CheckRequirements())
        {
            //Unlocks Anguana as an Unbound Entity
            //0 is a magic number, remove it later
            Debug.Log("Unbound Anguana Unlocked");
            GameObject unboundDeity = Instantiate(spawnableDeities[0]);
            Debug.Log("Moving Unbound Anguana to Starting Position");
            if (unboundDeity != null)
            {
                Debug.Log("Start of Summon Deity on Battlefield");
                //battleManager.currentBattleType = BattleType.battleWithDeity;
                //TileController deitySpawningTile = GridManager.Instance.GetTileControllerInstance(4, 8);
                //unboundDeity.transform.position = deitySpawningTile.transform.position;
                //unboundDeity.GetComponent<Unit>().ownedTile = deitySpawningTile;
                //deitySpawningTile.detectedUnit = unboundDeity.gameObject;
                //unboundDeity.GetComponent<Unit>().currentXCoordinate = deitySpawningTile.tileXCoordinate;
                //unboundDeity.GetComponent<Unit>().currentYCoordinate = deitySpawningTile.tileYCoordinate;
                //unboundDeity.gameObject.transform.localScale = new Vector3(3.5f, 3.5f, 3.5f);

                unboundDeity.GetComponent<Unit>().MoveUnit(3, 7);
                TileController deitySpawningTile = GridManager.Instance.GetTileControllerInstance(3, 7);
                unboundDeity.GetComponent<Unit>().ownedTile = deitySpawningTile;
                deitySpawningTile.detectedUnit = unboundDeity;

                foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
                {
                    Destroy(enemy);
                }
                unboundDeity.gameObject.tag = "Enemy";
            }
        }
        else
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
}
