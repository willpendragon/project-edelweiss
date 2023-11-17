using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTurnManager : MonoBehaviour
{
    public List<DummyEnemy> enemies;
    public int currentTurnIndex;

    void Start()
    {
        // Initialize your list of enemies
        //enemies = new List<DummyEnemy>();
        // Add all your enemy instances to the list here
    }

    public void StartTurns()
    {
        // Sort the enemies based on their speed in descending order
        enemies.Sort((a, b) => b.speed.CompareTo(a.speed));
        currentTurnIndex = 0;
        StartCoroutine(ExecuteTurns());
    }

    private IEnumerator ExecuteTurns()
    {
        while (currentTurnIndex < enemies.Count)
        {
            DummyEnemy currentEnemy = enemies[currentTurnIndex];
            Debug.Log("Current Turn: " + currentEnemy.name);

            // Implement your logic here for the enemy's turn.
            // You can handle things like moving, attacking, etc.

            // Wait for some time before moving to the next turn
            yield return new WaitForSeconds(1.0f);

            // Move to the next enemy in the list
            currentTurnIndex++;
        }

        // All enemies have finished their turns. You can handle end-of-round logic here.
        Debug.Log("Enemy turns are over.");
    }
}