using UnityEngine;

/// <summary>
/// An interactable component that, when attached to an object with a Unit component,
/// redirects any damage taken to the active Deity on the battlefield.
/// </summary>
[RequireComponent(typeof(Unit))]
public class DeityShard : MonoBehaviour
{
    private Deity _residentDeity;
    private Unit _unitComponent;
    [SerializeField] private BattleFeedbackController _battleFeedbackController;

    void Start()
    {
        _unitComponent = GetComponent<Unit>();

        // Find the active Deity in the scene via the BattleManager singleton.
        if (BattleManager.Instance != null && BattleManager.Instance.deity != null)
        {
            _residentDeity = BattleManager.Instance.deity;
        }
        else
        {
            Debug.LogWarning("DeityShard could not find an active Deity on the battlefield.", this);
        }

        // Subscribe to the damage event on its own Unit component.
        _unitComponent.OnTakenDamage.AddListener(HandleDamageTaken);
    }

    private void OnDestroy()
    {
        // Unsubscribe from the event to prevent memory leaks.
        if (_unitComponent != null)
        {
            _unitComponent.OnTakenDamage.RemoveListener(HandleDamageTaken);
        }
    }

    private void HandleDamageTaken(float damageAmount)
    {
        if (_residentDeity != null)
        {
            // Find the Unit component on the Deity's root GameObject.
            Unit deityUnit = _residentDeity.GetComponent<Unit>();
            if (deityUnit != null)
            {
                float maxHp = deityUnit.unitMaxHealthPoints;
                float previousHpPercentage = maxHp > 0 ? deityUnit.unitHealthPoints / maxHp : 0;

                deityUnit.TakeDamage(damageAmount);
                _residentDeity.UpdateDeityHealthBar();

                float currentHpPercentage = maxHp > 0 ? deityUnit.unitHealthPoints / maxHp : 0;

                Debug.Log($"Shard attacked, {damageAmount} damage on {deityUnit.unitTemplate.unitName}.");

                _residentDeity.deityCry.Play();
                _battleFeedbackController.PlayHitAnimation();
                // If the Boss is Moon Princess, use her public thresholds to trigger phase shift notifications
                if (_residentDeity.summoningBehaviour is DeityMoonPrincessBehavior moonPrincessBehavior)
                {
                    if (previousHpPercentage > moonPrincessBehavior.angryHpThreshold &&
                        currentHpPercentage <= moonPrincessBehavior.angryHpThreshold)
                    {
                        BattleInterface.Instance.SetDeityNotification(
                            $"{deityUnit.unitTemplate.unitName}'s rage makes it stronger!");
                    }
                    else if (previousHpPercentage > moonPrincessBehavior.veryAngryHpThreshold &&
                             currentHpPercentage <= moonPrincessBehavior.veryAngryHpThreshold)
                    {
                        BattleInterface.Instance.SetDeityNotification(
                            $"{deityUnit.unitTemplate.unitName} is furious! Its power intensifies!");
                    }
                }

                // Update the Deity Battle UI to reflect the new health value
                DeityBattleUIController uiController = FindAnyObjectByType<DeityBattleUIController>();
                if (uiController != null)
                {
                    uiController.UpdateUIValues();
                }
            }
            else
            {
                Debug.LogWarning("Linked Deity does not have a Unit component on its root object to take damage.",
                    _residentDeity);
            }
        }
    }
}