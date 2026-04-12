using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyProfileController : MonoBehaviour
{
    [SerializeField] BattleManager _battleManager;
    [SerializeField] List<Slider> _enemySliders = new List<Slider>();

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
}
