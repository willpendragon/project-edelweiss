using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyProfileController : MonoBehaviour
{
    [SerializeField] BattleManager _battleManager;
    [SerializeField] List<Slider> _enemySliders = new List<Slider>();
    
    // Dictionary mapping enemy GameObjects to their sliders for quick lookup
    private Dictionary<GameObject, Slider> _unitToSliderMap = new Dictionary<GameObject, Slider>();

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
                
                // Hide slider at battle start - only show on hover
                slider.gameObject.SetActive(false);
            }
        }
        GetSliderValues();
    }

    private void GetSliderValues()
    {
        if (_enemySliders.Count <= 0)
            return;

        foreach (var enemySlider in _enemySliders)
        {
            // Failsafe check in case a slider was destroyed or malformed
            if (enemySlider == null) continue;

            var parentUnit = enemySlider.GetComponentInParent<Unit>();
            if (parentUnit != null)
            {
                enemySlider.maxValue = parentUnit.unitHealthPoints;
                enemySlider.value = parentUnit.unitHealthPoints;
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
                Debug.Log($"[EnemySlider] Showing slider for {enemyUnit.name}");
            }
        }
        else
        {
            Debug.LogWarning($"[EnemySlider] No slider found in map for {enemyUnit.name}");
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
                Debug.Log($"[EnemySlider] Hiding slider for {enemyUnit.name}");
            }
        }
    }
}
