using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleFeedbackController : MonoBehaviour
{
    public GameObject playerUnit;
    public float restorePlayerUnitPositionCooldown;

    public void OnEnable()
    {
        Moveset.OnPlayerMeleeAttack += MovePlayerUnitNearEnemyTarget;
    }
    public void OnDisable()
    {
        Moveset.OnPlayerMeleeAttack -= MovePlayerUnitNearEnemyTarget;
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    void MovePlayerUnitNearEnemyTarget(Transform enemyTargetPosition)
    {
        playerUnit.transform.position = enemyTargetPosition.position;
        StartCoroutine("RestorePlayerUnitPosition");
        Debug.Log("Player Unit Melee Attack Feedback Test");
    }

    IEnumerator RestorePlayerUnitPosition()
    {
        yield return new WaitForSeconds(restorePlayerUnitPositionCooldown);
        Debug.Log("Restoring Player Unit Position");
        playerUnit.transform.position = playerUnit.GetComponent<Player>().unitCurrentTile.transform.position;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
