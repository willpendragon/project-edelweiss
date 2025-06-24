using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitsBuffingController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(BuffUnitsWrapper());
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator BuffUnitsWrapper()
    {
        yield return new WaitForSeconds(1f);
        BuffUnits();
    }

    void BuffUnits()
    {
        List<Unit> playerPartyMemberInstances = GameManager.Instance.playerPartyMembersInstances;
        GameObject[] currentBattleEnemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (var playerPartyMemberInstance in playerPartyMemberInstances)
        {
            // Quick implementation, for demo purposes only.
            if (playerPartyMemberInstance != null)
            {
                playerPartyMemberInstance.unitOpportunityPoints = 1000;
                playerPartyMemberInstance.unitMaxHealthPoints = 1000;
                playerPartyMemberInstance.unitManaPoints = 1000;
            }
        }

        foreach (var enemy in currentBattleEnemies)
        {
            if (enemy != null)
            {
                Unit enemyUnitComponent = enemy.GetComponent<Unit>();
                //enemyUnitComponent.unitOpportunityPoints = 1000;
                enemyUnitComponent.unitHealthPoints = 1000;
                enemyUnitComponent.unitMaxHealthPoints = 1000;
            }
        }
    }
}
