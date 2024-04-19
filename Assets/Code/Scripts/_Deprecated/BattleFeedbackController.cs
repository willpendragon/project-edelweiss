using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class BattleFeedbackController : MonoBehaviour
{
    public void PlayMeleeAttackAnimation(Unit activePlayerUnit, Unit currentTarget)
    {
        Debug.Log("Playing Attack Animation");
        Animator activePlayerUnitAnimator = activePlayerUnit.gameObject.GetComponentInChildren<Animator>();
        activePlayerUnitAnimator.SetTrigger("Attack");

        Vector3 originalPosition = activePlayerUnit.transform.position;


        activePlayerUnit.transform.position = currentTarget.transform.position;
        StartCoroutine(RestorePlayerUnitPosition(activePlayerUnit, originalPosition));
    }

    IEnumerator RestorePlayerUnitPosition(Unit activePlayerUnit, Vector3 originalPosition)
    {
        float timeBeforeRestoringPlayerUnitPosition = 0.5f;
        yield return new WaitForSeconds(timeBeforeRestoringPlayerUnitPosition);
        activePlayerUnit.transform.position = originalPosition;
    }

    public void PlayHurtAnimation()
    {
        Debug.Log("Playing Hurt Animation");
        Animator activePlayerUnitAnimator = GetComponentInChildren<Animator>();
        activePlayerUnitAnimator.SetTrigger("Hurt");
    }
}
