using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System;
using JetBrains.Annotations;

public class GameManager : MonoBehaviour
{
    public NodeController currentNode;
    public static GameManager Instance;
    public GameObject[] currentEnemySelection;
    public EnemySelection currentEnemySelectionComponent;
    public List<EnemyType> currentEnemySelectionIds;
    public List<Vector2> currentEnemySelectionCoords;
    public List<Unit> playerPartyMembers;
    public List<Unit> playerPartyMembersInstances;
    public List<Deity> collectibleDeities;

    public DeityLinkController deityLinkController;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InstantiateUnits();
            SceneManager.sceneLoaded += OnSceneLoaded; // Subscribe to the sceneLoaded event
        }
        else if (Instance != this)
        {
            Destroy(gameObject); // Ensure only one instance of GameManager exists
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // This method is now correctly subscribed to the SceneManager.sceneLoaded event
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Attempt to access the DeityLinkController component safely
        if (deityLinkController != null)
        {
            //deityLinkController.LoadGame(); // Call LoadGame only if the component is found
        }
        else
        {
            Debug.LogWarning("DeityLinkController component not found on GameManager GameObject.");
        }
    }
    public void MarkCurrentNodeAsCompleted()

    {
        currentNode.nodeCompleted = true;
    }
    public void DefineEnemySelection(GameObject[] enemySelection)
    {
        //currentEnemySelection = enemySelection;
    }
    public void UpdateEnemyData(Level level)
    {
        currentEnemySelectionIds.Clear();
        currentEnemySelectionCoords.Clear();

        for (int i = 0; i < level.EnemyTypeIds.Count; i++)
        {
            currentEnemySelectionIds.Add(level.EnemyTypeIds[i]);
            currentEnemySelectionCoords.Add(level.UnitCoordinates[i]);
        }
    }

    public void InstantiateUnits()
    {
        // Clear the existing instances list
        playerPartyMembersInstances.Clear();

        // Go through all the prefabs and instantiate them
        foreach (var unitPrefab in playerPartyMembers)
        {
            Unit newUnitInstance = Instantiate(unitPrefab, this.gameObject.transform);
            playerPartyMembersInstances.Add(newUnitInstance); // Add the new instance to the list
        }
        Debug.Log("Instantiated Player Units");
        ApplyDeityLinks();

    }
    public void ApplyDeityLinks()
    {
        Dictionary<string, string> unitsLinkedToDeities = SaveStateManager.saveData.unitsLinkedToDeities;
        foreach (var entry in unitsLinkedToDeities)
        {
            string unitID = entry.Key;
            string deityID = entry.Value;
        }

        foreach (var unitPrefab in playerPartyMembersInstances)
        {
            unitsLinkedToDeities.TryGetValue(unitPrefab.GetComponent<Unit>().Id, out string connectedDeity);
            unitPrefab.GetComponent<Unit>().LinkedDeityId = connectedDeity;
            unitPrefab.GetComponent<Unit>().linkedDeity = GameManager.Instance.collectibleDeities.Find(deity => deity.Id == unitPrefab.LinkedDeityId);
        }
    }
}
