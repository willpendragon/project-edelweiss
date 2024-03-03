using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public NodeController currentNode;
    public static GameManager Instance; // Changed from _instance to follow the common Singleton naming convention
    public GameObject[] currentEnemySelection;
    public EnemySelection currentEnemySelectionComponent;
    public List<EnemyType> currentEnemySelectionIds;
    public List<Vector2> currentEnemySelectionCoords;
    public List<Unit> playerPartyMembers;
    public List<Unit> playerPartyMembersInstances;
    public List<Deity> capturedDeities;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded; // Subscribe to the sceneLoaded event
        }
        else if (Instance != this)
        {
            Destroy(gameObject); // Ensure only one instance of GameManager exists
        }
    }

    private void OnDestroy()
    {
        // It's good practice to unsubscribe from the event when the GameObject is destroyed.
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // This method is now correctly subscribed to the SceneManager.sceneLoaded event
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Attempt to access the DeityLinkController component safely
        DeityLinkController deityLinkController = GetComponent<DeityLinkController>();
        if (deityLinkController != null)
        {
            deityLinkController.LoadGame(); // Call LoadGame only if the component is found
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
            Unit newUnitInstance = Instantiate(unitPrefab);
            newUnitInstance.Id = System.Guid.NewGuid().ToString(); // Set a new ID or any other initialization
            playerPartyMembersInstances.Add(newUnitInstance); // Add the new instance to the list
        }
    }
}