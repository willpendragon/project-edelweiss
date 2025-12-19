using System.Collections;
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
    public Animator comboIncreaseVFXAnimator;
    public Animator unitAnimator;
    public GameObject buffIcon;

    [SerializeField] Animator hitAnimator;


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
        Vector3 destination;

        if (currentTarget.unitType == Unit.UnitType.Deity)
        {
            var deitySpawner = FindAnyObjectByType<DeitySpawner>();
            destination = deitySpawner.DeityObeliskSpawningPoint.transform.position;

            float stopDistance = 1.2f;
            float yOffset = 0.3f; // positive = up, negative = down

            Vector3 direction = (destination - originalPosition).normalized;
            Vector3 offsetDestination = destination - direction * stopDistance;

            offsetDestination.y += yOffset;

            activePlayerUnit.transform.position = offsetDestination;
            deitySpawner.ShowObeliskDamageFeedback();
        }
        else
        {
            destination = currentTarget.transform.position;
            activePlayerUnit.transform.position = destination;
        }
        StartCoroutine(RestorePlayerUnitPosition(activePlayerUnit, originalPosition));
    }

    IEnumerator RestorePlayerUnitPosition(Unit activePlayerUnit, Vector3 originalPosition)
    {
        float timeBeforeRestoringPlayerUnitPosition = 0.5f;
        yield return new WaitForSeconds(timeBeforeRestoringPlayerUnitPosition);
        activePlayerUnit.transform.position = originalPosition;
    }

    public void DisplaySpellObeliskDamageFeedback(Unit activePlayerUnit)
    {
        var deitySpawner = FindAnyObjectByType<DeitySpawner>();
        deitySpawner.ShowObeliskDamageFeedback();
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

    public void PlayHitAnimation()
    {
        Debug.Log("Playing Hit Animation");
        hitAnimator.SetTrigger("Hit");
    }

    public void PlayUnitDeathAnimationVFX()
    {
        if (unitAnimator != null)
        {
            unitAnimator.SetTrigger("Die");
            DeleteExistingVFX();
        }

        if (PlayDeathVFX != null)
        {
            deathDisappearAnimationVFX = Instantiate(deathDisappearAnimationVFX, gameObject.transform);
            deathDisappearAnimationVFX.GetComponent<Animator>().SetTrigger("TriggerDeathVFX");
        }
    }

    private void DeleteExistingVFX()
    {
        // Destroy status VFX after the Unit's death (example: Frozen Cube)
        var VFX = gameObject.GetComponentInChildren<StatusVFX>();
        if (VFX != null)
        {
            VFX.DestroyVFX();
        }
    }

    public void PlayComboIncreaseVFX()
    {
        if (comboIncreaseVFXAnimator != null)
        {
            comboIncreaseVFXAnimator.SetTrigger("ComboCounterIncrease");
        }
    }
}
