using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class RoamingDeityController : MonoBehaviour
{
    [Header("Dependencies")]
    public OverworldMapGenerator mapGenerator;
    public GameObject deityModelPrefab;
    
    [Header("Settings")]
    public int spawnNodeDistanceMin = 3; // Ensure it doesn't spawn right on top of the player
    public MapData simildeBossMapData; // Assign the specific MapData/EnemyPartyData for Similde
    
    [Header("Visuals")]
    [Tooltip("The final scale of the Deity model on the overworld map.")]
    public float deityModelScale = 0.5f; 
    [Tooltip("How long the apparition animation takes.")]
    public float spawnTweenDuration = 0.8f;

    private GameObject _spawnedDeityInstance;
    private int _deityCurrentNodeId = -1;
    private bool _isDeityActive = false;

    private void Start()
    {
        // For prototyping, we can spawn the Deity a few seconds after the map is generated,
        // or you can call SpawnDeity() via a specific event (e.g., from GameFlowController).
        Invoke(nameof(SpawnDeity), 2f);
    }

    /// <summary>
    /// Randomly spawns the Deity on a valid, un-cleared node.
    /// </summary>
    public void SpawnDeity()
    {
        if (mapGenerator == null || mapGenerator.domains.Count == 0 || _isDeityActive)
            return;

        int playerStartId = mapGenerator.currentNodeId;
        int maxNodeCount = mapGenerator.currentMapNodeTransform != null ? 
            mapGenerator.spawnedNodes.Count : 10; // Fallback

        // Find a valid node ID to spawn the deity
        List<int> validSpawnNodes = new List<int>();
        for (int i = 0; i < maxNodeCount; i++)
        {
            if (Mathf.Abs(i - playerStartId) >= spawnNodeDistanceMin)
            {
                validSpawnNodes.Add(i);
            }
        }

        if (validSpawnNodes.Count > 0)
        {
            _deityCurrentNodeId = validSpawnNodes[Random.Range(0, validSpawnNodes.Count)];
            
            // Wait for map generator node positions to populate
            Vector3 targetPosition = mapGenerator.spawnedNodes[_deityCurrentNodeId].transform.position;

            _spawnedDeityInstance = Instantiate(deityModelPrefab, targetPosition + Vector3.up * 0.5f, Quaternion.identity);
            _isDeityActive = true;

            // --- Apparition Tween ---
            // Set scale to 0 initially, then tween up to the target scale to pop in
            _spawnedDeityInstance.transform.localScale = Vector3.zero;
            _spawnedDeityInstance.transform.DOScale(Vector3.one * deityModelScale, spawnTweenDuration)
                .SetEase(Ease.OutBack);
            
            Debug.Log($"[Roaming Deity] Similde spawned at Node {_deityCurrentNodeId}");
        }
    }

    /// <summary>
    /// Call this immediately after the Player completes their move to a new node.
    /// </summary>
    public void OnPlayerMoved(int targetId)
    {
        if (!_isDeityActive) return;

        StartCoroutine(DeityMoveRoutine());
    }

    private IEnumerator DeityMoveRoutine()
    {
        int playerNodeId = mapGenerator.currentNodeId;

        // Simple approach: Deity just moves 1 node closer by ID, or use pathfinding if available.
        // Assuming higher node ID = further forward. If the player is ahead, the deity moves up.
        int nextNodeId = _deityCurrentNodeId;

        if (_deityCurrentNodeId < playerNodeId)
        {
            nextNodeId++;
        }
        else if (_deityCurrentNodeId > playerNodeId)
        {
            nextNodeId--; // Moves backwards
        }

        _deityCurrentNodeId = nextNodeId;
        Vector3 nextPosition = mapGenerator.spawnedNodes[_deityCurrentNodeId].transform.position;

        // Animate the Deity moving
        yield return _spawnedDeityInstance.transform.DOMove(nextPosition + Vector3.up * 0.5f, 0.5f)
            .SetEase(Ease.InOutSine)
            .WaitForCompletion();

        CheckForEncounter();
    }

    private void CheckForEncounter()
    {
        if (_deityCurrentNodeId == mapGenerator.currentNodeId)
        {
            TriggerForcedDeityBattle();
        }
    }

    private void TriggerForcedDeityBattle()
    {
        Debug.Log("[Roaming Deity] Encountered the Player! Forcing Battle...");
        
        // Remove or sink the map model
        _isDeityActive = false;
        Destroy(_spawnedDeityInstance);

        // Fetch the MapNodeController the player is standing on to inject the forced boss data
        MapNodeController currentNode = mapGenerator.spawnedNodes[mapGenerator.currentNodeId].GetComponentInChildren<MapNodeController>();
        EnemySelection currentSelection = currentNode.GetComponentInParent<EnemySelection>();

        if (currentSelection != null && simildeBossMapData != null)
        {
            // Override the node's resident encounter with Similde
            currentSelection.mapData = simildeBossMapData; 
            // Also override the type so it's treated as a Boss/Miniboss to block escape if your system checks map type
            currentNode.type = NodeType.BossBattle; 
            
            // Trigger the node logic immediately as if clicked
            currentNode.HandleBattleEntry();
        }
        else
        {
            Debug.LogError("[Roaming Deity] Missing required components or MapData to force battle.");
        }
    }
}