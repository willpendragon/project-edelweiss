using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BattleFeedbackController : MonoBehaviour
{

    [Header("Player Actions SFX")]

    public UnityEvent PlayMeleeAttackSFX;
    public UnityEvent PlaySpellSFX;
    public UnityEvent PlayMovementSelectedSFX;
    public UnityEvent PlayMovementConfirmedSFX;
    public UnityEvent PlayDamageSFX;
    public UnityEvent PlayPlaceCrystalSFX;
    public UnityEvent PlayPrayerSFX;

    [Header("Unit Management SFX")]

    public UnityEvent PlaySelectionSFX;
    public UnityEvent PlaySelectionWaitingConfirmationSFX;
    public UnityEvent PlayDeselectionSFX;


    [Header("Visuals")]

    public UnityEvent PlayDeathVFX;
    public GameObject deathDisappearAnimationVFX;
    public Animator unitAnimator;
    public GameObject buffIcon;


    public void PlayMeleeAttackAnimation(Unit activePlayerUnit, Unit currentTarget)
    {
        Debug.Log("Playing Attack Animation");
        Animator activePlayerUnitAnimator = activePlayerUnit.gameObject.GetComponentInChildren<Animator>();
        activePlayerUnitAnimator.SetTrigger("Attack");
        if (PlayMeleeAttackSFX != null)
        {
            PlayMeleeAttackSFX.Invoke();
        }

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
        if (PlayDamageSFX != null)
        {
            PlayDamageSFX.Invoke();
        }
    }

    public void PlayUnitDeathAnimationVFX()
    {
        if (unitAnimator != null)
        {
            unitAnimator.SetTrigger("Die");
        }

        if (PlayDeathVFX != null)
        {
            deathDisappearAnimationVFX = Instantiate(deathDisappearAnimationVFX, gameObject.transform);
            deathDisappearAnimationVFX.GetComponent<Animator>().SetTrigger("TriggerDeathVFX");
        }
    }
}
