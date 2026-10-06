using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class RoamingDeityController : MonoBehaviour
{
    [Header("Dependencies")]
    public OverworldMapGenerator mapGenerator;
    public GameObject deityModelPrefab;
    public GameObject deityBattlePrefab;

    [Header("Settings")]
    public int spawnNodeDistanceMin = 3;
    [Tooltip("Be careful to assign a MapData of the Regular Battle type! Other types (like Puzzle) will just ignore the forced Deity spawn. To be fixed 'later'...")]
    public MapData simildeBossMapData;

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
        Invoke(nameof(SpawnDeity), 2f);
    }

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

            // Wait for map generator node positions to populate.
            Vector3 targetPosition = mapGenerator.spawnedNodes[_deityCurrentNodeId].transform.position;

            _spawnedDeityInstance = Instantiate(deityModelPrefab, targetPosition + Vector3.up * 0.5f, Quaternion.identity);
            _isDeityActive = true;

            _spawnedDeityInstance.transform.localScale = Vector3.zero;
            _spawnedDeityInstance.transform.DOScale(Vector3.one * deityModelScale, spawnTweenDuration)
                .SetEase(Ease.OutBack);

            Debug.Log($"[Roaming Deity] Similde spawned at Node {_deityCurrentNodeId}");
        }
    }

    public void OnPlayerMoved(int targetId)
    {
        if (!_isDeityActive) return;

        StartCoroutine(DeityMoveRoutine());
    }

    private IEnumerator DeityMoveRoutine()
    {
        int playerNodeId = mapGenerator.currentNodeId;

        // Already sharing the player's node: skip movement and resolve the encounter.
        if (_deityCurrentNodeId == playerNodeId)
        {
            CheckForEncounter();
            yield break;
        }
        List<int> path = FindPath(_deityCurrentNodeId, playerNodeId);

        if (path == null || path.Count < 2)
        {
            Debug.LogWarning($"[Roaming Deity] No connected path from Node {_deityCurrentNodeId} to Node {playerNodeId}. Staying put.");
            yield break;
        }

        // Move exactly ONE step along the connected path toward the player.
        _deityCurrentNodeId = path[1];
        Vector3 nextPosition = mapGenerator.spawnedNodes[_deityCurrentNodeId].transform.position;

        yield return _spawnedDeityInstance.transform.DOMove(nextPosition + Vector3.up * 0.5f, 0.5f)
            .SetEase(Ease.InOutSine)
            .WaitForCompletion();

        CheckForEncounter();
    }

    private List<int> FindPath(int start, int target)
    {
        if (mapGenerator == null || mapGenerator.adjacencyList == null)
            return null;

        if (!mapGenerator.adjacencyList.ContainsKey(start) || !mapGenerator.adjacencyList.ContainsKey(target))
            return null;

        Queue<int> frontier = new Queue<int>();
        Dictionary<int, int> parentMap = new Dictionary<int, int>();

        frontier.Enqueue(start);
        parentMap[start] = -1;

        while (frontier.Count > 0)
        {
            int current = frontier.Dequeue();

            if (current == target) break;

            foreach (int neighbor in mapGenerator.adjacencyList[current])
            {
                if (!parentMap.ContainsKey(neighbor))
                {
                    parentMap[neighbor] = current;
                    frontier.Enqueue(neighbor);
                }
            }
        }

        if (!parentMap.ContainsKey(target))
            return null;

        List<int> path = new List<int>();
        int backtrackNode = target;
        while (backtrackNode != -1)
        {
            path.Add(backtrackNode);
            backtrackNode = parentMap[backtrackNode];
        }
        path.Reverse();
        return path;
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

        _isDeityActive = false;
        Destroy(_spawnedDeityInstance);

        MapNodeController currentNode = mapGenerator.spawnedNodes[mapGenerator.currentNodeId].GetComponentInChildren<MapNodeController>();
        EnemySelection currentSelection = currentNode.GetComponentInParent<EnemySelection>();

        if (currentSelection != null && simildeBossMapData != null)
        {
            currentSelection.mapData = simildeBossMapData;
            currentNode.type = NodeType.BossBattle;

            BattleTypeController.isForcedRoamingDeity = true;
            BattleTypeController.forcedRoamingDeityPrefab = deityBattlePrefab;

            currentNode.HandleBattleEntry();
        }
        else
        {
            Debug.LogError("[Roaming Deity] Missing required components or MapData to force battle.");
        }
    }
}