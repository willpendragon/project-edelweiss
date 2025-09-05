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
            _enemySliders.Add(enemy.GetComponentInChildren<Slider>());
        }
        GetSliderValues();
    }

    private void GetSliderValues()
    {
        if (_enemySliders.Count <= 0)
            return;
        foreach (var enemySlider in _enemySliders)
        {
            enemySlider.maxValue = enemySlider.GetComponentInParent<Unit>().unitHealthPoints;
            enemySlider.value = enemySlider.GetComponentInParent<Unit>().unitHealthPoints;
        }
    }
}
