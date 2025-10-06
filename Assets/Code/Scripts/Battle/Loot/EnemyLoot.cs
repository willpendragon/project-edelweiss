using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyLoot : MonoBehaviour
{
    [SerializeField] private Ingredient _ingredient;
    public Ingredient RollLootChance()
    {
        int minValue = 1;
        int maxValue = 7;
        int lootTreshold = 1;

        int rolledNumber = Random.Range(minValue, maxValue);
        if (rolledNumber >= lootTreshold)
        {
            return _ingredient;
        }
        else
        {
            return null;
        }
    }
}
