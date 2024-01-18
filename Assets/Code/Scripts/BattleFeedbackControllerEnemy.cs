using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleFeedbackControllerEnemy : MonoBehaviour
{
    public GameObject enemyUnit;
    public float restoreEnemyUnitPositionCooldown;

    public void MoveEnemyUnitNearPlayerTarget(Transform playerUnitTargetTransform)
    {
        enemyUnit.transform.position = playerUnitTargetTransform.position;
        StartCoroutine("RestoreEnemyUnitPosition");
        Debug.Log("Enemy Unit Melee Attack Feedback Test");
    }

    IEnumerator RestoreEnemyUnitPosition()
    {
        yield return new WaitForSeconds(restoreEnemyUnitPositionCooldown);
        Debug.Log("Restoring Enemy Unit Position");
        this.gameObject.transform.position = enemyUnit.GetComponent<EnemyAgent>().enemyOriginalPosition;
        //enemyUnit.transform.position
    }

}
