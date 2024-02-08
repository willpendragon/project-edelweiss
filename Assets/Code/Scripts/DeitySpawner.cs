using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeitySpawner : MonoBehaviour
{
    [SerializeField] GameObject[] spawnableDeities;
    [SerializeField] Transform deitySpawnPosition;
    [SerializeField] DeityAchievementsController deityAchievementsController;
    // Start is called before the first frame update
    void Start()
    {
        if (deityAchievementsController.CheckRequirements())
        {
            GameObject unboundDeity = Instantiate(spawnableDeities[0], deitySpawnPosition);
            GameObject.FindGameObjectWithTag("BattleManager").GetComponent<BattleManager>().deity = unboundDeity.GetComponent<Deity>();
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
