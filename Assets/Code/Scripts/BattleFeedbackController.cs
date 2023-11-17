using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class BattleFeedbackController : MonoBehaviour
{
    public GameObject playerUnit;
    public float restorePlayerUnitPositionCooldown;
    public GameObject castSpellVFX;
    public PlayableDirector mainCharacterPlayableDirector;
    public Vector3 unitOriginalPosition;

    public void OnEnable()
    {
        Moveset.OnPlayerMeleeAttack += MovePlayerUnitNearEnemyTarget;
    }
    public void OnDisable()
    {
        Moveset.OnPlayerMeleeAttack -= MovePlayerUnitNearEnemyTarget;
    }
    public void Start()
    {
        unitOriginalPosition = this.gameObject.transform.position;
    }
    void MovePlayerUnitNearEnemyTarget(Transform enemyTargetPosition)
    {
        Vector3 playerUnitPosition = new Vector3(enemyTargetPosition.position.x, playerUnit.transform.position.y, enemyTargetPosition.transform.position.z);
        playerUnit.transform.position = playerUnitPosition;
        mainCharacterPlayableDirector.Play();
        StartCoroutine("RestorePlayerUnitPosition");
        Debug.Log("Player Unit Melee Attack Feedback Test");
    }
    IEnumerator RestorePlayerUnitPosition()
    {
        yield return new WaitForSeconds(restorePlayerUnitPositionCooldown);
        Debug.Log("Restoring Player Unit Position");
        playerUnit.transform.position = unitOriginalPosition;
        //playerUnit.GetComponent<Player>().unitCurrentTile.transform.position;
    }
    public void TriggerCastSpellVFX()
    {
        GameObject castSpellVFXInstance = GameObject.Instantiate(castSpellVFX, this.transform);
        StartCoroutine(DestroyTriggerCastSpellVFX(castSpellVFXInstance));
    }
    IEnumerator DestroyTriggerCastSpellVFX(GameObject castSpellVFXInstance)
    {
        yield return new WaitForSeconds(1);
        Destroy(castSpellVFXInstance);
    }
}
