using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class EnemyProfileController : MonoBehaviour
{
    [SerializeField] BattleManager _battleManager;
    [SerializeField] List<Slider> _enemySliders = new List<Slider>();
    
    // Dictionary mapping enemy GameObjects to their sliders for quick lookup
    private Dictionary<GameObject, Slider> _unitToSliderMap = new Dictionary<GameObject, Slider>();
    // Dictionary mapping enemy Units to their health change listeners
    private Dictionary<Unit, UnityAction<float>> _healthChangeListeners = new Dictionary<Unit, UnityAction<float>>();

    public static EnemyProfileController Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Start()
    {
        // Using coroutine, to be optimized later
        StartCoroutine(Initialize());
    }

    IEnumerator Initialize()
    {
        yield return new WaitForSeconds(0.5f);
        var enemies = _battleManager.enemiesOnBattlefield;
        
        foreach (var enemy in enemies)
        {
            var slider = enemy.GetComponentInChildren<Slider>();
            if (slider != null)
            {
                _enemySliders.Add(slider);
                _unitToSliderMap[enemy.gameObject] = slider;
                
                // Get the Unit component and subscribe to health changes
                Unit enemyUnit = enemy.GetComponent<Unit>();
                if (enemyUnit != null)
                {
                    SubscribeToEnemyHealthChanges(enemyUnit, slider);
                }
                
                // Hide slider at battle start - only show on hover
                slider.gameObject.SetActive(false);
            }
        }
        
        GetSliderValues();
    }

    /// <summary>
    /// Subscribes to health change events for an enemy unit.
    /// </summary>
    private void SubscribeToEnemyHealthChanges(Unit enemyUnit, Slider slider)
    {
        if (enemyUnit == null || slider == null)
            return;

        // Create a listener for this unit's health changes
        UnityAction<float> healthChangeListener = (newHealth) =>
        {
            if (slider != null)
            {
                slider.value = newHealth;
            }
        };

        // Store the listener so we can unsubscribe later if needed
        _healthChangeListeners[enemyUnit] = healthChangeListener;

        // Subscribe to the unit's health change event
        enemyUnit.onHealthChanged.AddListener(healthChangeListener);

        Debug.Log($"[EnemyProfileController] Subscribed to health changes for {enemyUnit.name}");
    }

    private void GetSliderValues()
    {
        if (_enemySliders.Count <= 0)
            return;

        foreach (var enemySlider in _enemySliders)
        {
            // Failsafe check in case a slider was destroyed or malformed
            if (enemySlider == null) 
                continue;

            var parentUnit = enemySlider.GetComponentInParent<Unit>();
            if (parentUnit != null)
            {
                // Set max value to max health
                enemySlider.maxValue = parentUnit.unitMaxHealthPoints;
                // Initialize current value
                enemySlider.value = parentUnit.unitHealthPoints;
                
                Debug.Log($"[EnemyProfileController] Initialized slider for {parentUnit.name}: {parentUnit.unitHealthPoints} / {parentUnit.unitMaxHealthPoints}");
            }
        }
    }

    /// <summary>
    /// Shows the HP slider for a specific enemy unit.
    /// </summary>
    public void ShowEnemySlider(GameObject enemyUnit)
    {
        if (enemyUnit == null)
            return;

        if (_unitToSliderMap.TryGetValue(enemyUnit, out Slider slider))
        {
            if (slider != null)
            {
                slider.gameObject.SetActive(true);
                
                // Ensure the slider value is up-to-date before showing
                Unit unit = enemyUnit.GetComponent<Unit>();
                if (unit != null)
                {
                    slider.value = unit.unitHealthPoints;
                }
                
                Debug.Log($"[EnemyProfileController] Showing slider for {enemyUnit.name}");
            }
        }
        else
        {
            Debug.LogWarning($"[EnemyProfileController] No slider found in map for {enemyUnit.name}");
        }
    }

    /// <summary>
    /// Hides the HP slider for a specific enemy unit.
    /// </summary>
    public void HideEnemySlider(GameObject enemyUnit)
    {
        if (enemyUnit == null)
            return;

        if (_unitToSliderMap.TryGetValue(enemyUnit, out Slider slider))
        {
            if (slider != null)
            {
                slider.gameObject.SetActive(false);
                Debug.Log($"[EnemyProfileController] Hiding slider for {enemyUnit.name}");
            }
        }
    }

    /// <summary>
    /// Unsubscribes from a unit's health changes (call when unit is destroyed).
    /// </summary>
    public void UnsubscribeFromEnemyHealthChanges(Unit enemyUnit)
    {
        if (enemyUnit == null)
            return;

        if (_healthChangeListeners.TryGetValue(enemyUnit, out var listener))
        {
            enemyUnit.onHealthChanged.RemoveListener(listener);
            _healthChangeListeners.Remove(enemyUnit);
            Debug.Log($"[EnemyProfileController] Unsubscribed from health changes for {enemyUnit.name}");
        }
    }

    /// <summary>
    /// Cleans up all listeners when the controller is destroyed.
    /// </summary>
    private void OnDestroy()
    {
        foreach (var kvp in _healthChangeListeners)
        {
            if (kvp.Key != null)
            {
                kvp.Key.onHealthChanged.RemoveListener(kvp.Value);
            }
        }
        _healthChangeListeners.Clear();
    }
}
