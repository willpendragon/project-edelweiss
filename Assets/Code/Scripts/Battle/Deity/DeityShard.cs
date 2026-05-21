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

    /// <summary>
    /// Handles the damage event and redirects the damage to the linked Deity.
    /// </summary>
    /// <param name="damageAmount">The amount of damage received, before mitigation.</param>
    private void HandleDamageTaken(float damageAmount)
    {
        if (_residentDeity != null)
        {
            // Find the Unit component on the Deity's root GameObject.
            Unit deityUnit = _residentDeity.GetComponent<Unit>();
            if (deityUnit != null)
            {
                deityUnit.TakeDamage(damageAmount);
                _residentDeity.UpdateDeityHealthBar();
                BattleInterface.Instance.SetDeityNotification($"Shard attacked, {damageAmount} damage on {deityUnit.unitTemplate.unitName}.");
                Debug.Log($"Shard attacked, {damageAmount} damage on {deityUnit.unitTemplate.unitName}.");
            }
            else
            {
                Debug.LogWarning("Linked Deity does not have a Unit component on its root object to take damage.", _residentDeity);
            }
        }
    }
}