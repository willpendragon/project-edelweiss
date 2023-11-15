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

    public void OnEnable()
    {
        Moveset.OnPlayerMeleeAttack += MovePlayerUnitNearEnemyTarget;
    }
    public void OnDisable()
    {
        Moveset.OnPlayerMeleeAttack -= MovePlayerUnitNearEnemyTarget;
    }
    void MovePlayerUnitNearEnemyTarget(Transform enemyTargetPosition)
    {
        playerUnit.transform.position = enemyTargetPosition.position;
        mainCharacterPlayableDirector.Play();
        StartCoroutine("RestorePlayerUnitPosition");
        Debug.Log("Player Unit Melee Attack Feedback Test");
    }
    IEnumerator RestorePlayerUnitPosition()
    {
        yield return new WaitForSeconds(restorePlayerUnitPositionCooldown);
        Debug.Log("Restoring Player Unit Position");
        playerUnit.transform.position = playerUnit.GetComponent<Player>().unitCurrentTile.transform.position;
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
