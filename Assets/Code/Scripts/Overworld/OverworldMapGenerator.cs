using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum NodeType
{
    RegularBattle,
    PuzzleBattle,
    MinibossBattle,
    BossBattle
}

public class OverworldMapGenerator : MonoBehaviour
{
    public List<Domain> domains = new List<Domain>();
    public GameObject mapNode;
    public Transform mapNodeTransform;

    [Header("Configuration")]
    [Tooltip("Assegna qui la tua configurazione ScriptableObject per regole e pesi.")]
    public MapGenerationConfig config;

    // --- NEW: Map Data Pools ---
    [Header("Map Data Pools")]
    [Tooltip("Maps to randomly select from based on the generated Node Type.")]
    public List<MapData> regularMaps = new List<MapData>();
    public List<MapData> puzzleMaps = new List<MapData>();
    public List<MapData> minibossMaps = new List<MapData>();
    public List<MapData> bossMaps = new List<MapData>();

    // --- NEW: Difficulty Rules ---
    [Header("Difficulty Progression")]
    public bool enforceDifficultyProgression = true;
    [Tooltip("Nodes up to this index will exclusively pull Easy maps.")]
    public int maxEasyNodeIndex = 3;
    [Tooltip("Nodes after Easy and up to this index will pull Medium maps. Anything higher pulls Hard maps.")]
    public int maxMediumNodeIndex = 7;

    [Header("Visuals & UI")]
    [Tooltip("Materiale per la linea del percorso (evita il bug della linea fucsia)")]
    public Material pathLineMaterial;
    [Tooltip("Alziamo leggermente la linea generata in modo che non si compenetri col pavimento")]
    public float lineVerticalOffset = 0.2f; 
    
    public GameObject[] partyMemberIcons;
    public float iconZOffset = 1f;

    [Header("Debug & Runtime Edit")]
    [Tooltip("Se attivo, le modifiche all'inspector rigenereranno la mappa in tempo reale")]
    public bool autoUpdateInPlayMode = true;

    [HideInInspector] public Transform currentMapNodeTransform;
    [HideInInspector] public int currentNodeId; 

    private List<Vector3> nodePositions = new List<Vector3>();
    private int currentDomainId = 0;

    // --- Variabili per il tracciamento e la distruzione ---
    private Domain currentDomain;
    private List<GameObject> spawnedNodes = new List<GameObject>();
    private List<GameObject> spawnedPartyIcons = new List<GameObject>();
    private List<GameObject> spawnedLines = new List<GameObject>();
    private bool needsRegeneration = false;
    
    private Dictionary<int, List<int>> adjacencyList = new Dictionary<int, List<int>>();
    private bool isMoving = false;

    private float lastMapWidth;
    private float lastMapDepth;
    private float lastMinDistance;
    private int lastSeed;
    
    // --- NEW: Tracking config weights and thresholds ---
    private float lastRegularWeight;
    private float lastPuzzleWeight;
    private float lastMinibossWeight;
    private float lastBossWeight;
    private int lastPuzzleThreshold;
    private int lastMinibossThreshold;

    private void Start()
    {
        if (config != null)
        {
            RecordCurrentConfigState();
        }
    }

    private void RecordCurrentConfigState()
    {
        if (config != null)
        {
            lastMapWidth = config.mapWidth;
            lastMapDepth = config.mapDepth;
            lastMinDistance = config.minDistanceApart;
            lastSeed = config.randomSeed;
            
            // --- NEW ---
            lastRegularWeight = config.regularBattleWeight;
            lastPuzzleWeight = config.puzzleBattleWeight;
            lastMinibossWeight = config.minibossBattleWeight;
            lastBossWeight = config.bossBattleWeight;
            lastPuzzleThreshold = config.puzzleBattleThreshold;
            lastMinibossThreshold = config.minibossBattleThreshold;
        }
    }

    private bool HasConfigChanged()
    {
        if (config == null) return false;

        return lastMapWidth != config.mapWidth ||
               lastMapDepth != config.mapDepth ||
               lastMinDistance != config.minDistanceApart ||
               lastSeed != config.randomSeed ||
               // --- NEW ---
               lastRegularWeight != config.regularBattleWeight ||
               lastPuzzleWeight != config.puzzleBattleWeight ||
               lastMinibossWeight != config.minibossBattleWeight ||
               lastBossWeight != config.bossBattleWeight ||
               lastPuzzleThreshold != config.puzzleBattleThreshold ||
               lastMinibossThreshold != config.minibossBattleThreshold;
    }

    private void Update()
    {
        // Controllo live per il debug in Play Mode
        if (Application.isPlaying && autoUpdateInPlayMode && config != null && currentDomain != null)
        {
            if (HasConfigChanged() || needsRegeneration)
            {
                RecordCurrentConfigState();
                needsRegeneration = false;
                RegenerateMap();
            }
        }
    }

    private void OnValidate()
    {
        if (Application.isPlaying && autoUpdateInPlayMode)
        {
            needsRegeneration = true;
        }
    }

    public void GenerateLevel(Domain domainLevelSelection)
    {
        if (config == null)
        {
            Debug.LogError("MapGenerationConfig is missing! Please assign one in the inspector.");
            return;
        }

        currentDomain = domainLevelSelection; 
        
        Random.InitState(config.randomSeed);
        Vector3 initialPosition = mapNodeTransform.position;

        GameSaveData gameSaveData = SaveStateManager.LoadGame();
        int highestUnlockedLevel = gameSaveData.highestUnlockedLevel;

        List<Vector3> scatteredPositions = new List<Vector3>();
        adjacencyList.Clear();

        // 1. Scatter nodes avoiding overlapping
        for (int i = 0; i < domainLevelSelection.levelList.Length; i++)
        {
            Vector3 testPosition = Vector3.zero;
            bool isValid = false;
            int attempt = 0;

            while (!isValid && attempt < 500)
            {
                attempt++;
                float randomX = Random.Range(0, config.mapWidth);
                float randomZ = Random.Range(-config.mapDepth / 2f, config.mapDepth / 2f);
                testPosition = initialPosition + new Vector3(randomX, 0, randomZ);

                isValid = true;
                
                foreach (Vector3 pos in scatteredPositions)
                {
                    if (Vector3.Distance(testPosition, pos) < config.minDistanceApart)
                    {
                        isValid = false;
                        break;
                    }
                }
            }

            scatteredPositions.Add(testPosition);
            adjacencyList[i] = new List<int>(); // Build up our graph representation mapping.
        }

        // 2. Sort from left to right to build an advancing mesh
        scatteredPositions.Sort((a, b) => a.x.CompareTo(b.x));
        
        // 3. Pre-generate node types to identify gateways (choke points) early
        NodeType[] predefinedNodeTypes = new NodeType[scatteredPositions.Count];
        for (int i = 0; i < scatteredPositions.Count; i++)
        {
            predefinedNodeTypes[i] = GenerateNodeType(i, scatteredPositions.Count);
        }

        // 4. Determine mesh connections (multi-path) restricted by Gateways
        List<Vector2Int> connections = new List<Vector2Int>();
        for (int i = 0; i < scatteredPositions.Count - 1; i++) // Skip the last node since it has no forward neighbors
        {
            // Find the closest gateway node ahead of the current node
            int nextGateway = scatteredPositions.Count - 1; // Default to the very last node if no gateway is found
            for (int g = i + 1; g < scatteredPositions.Count; g++)
            {
                if (predefinedNodeTypes[g] == NodeType.MinibossBattle || predefinedNodeTypes[g] == NodeType.BossBattle)
                {
                    nextGateway = g;
                    break;
                }
            }

            List<int> forwardNeighbors = new List<int>();
            // You can only connect to nodes up to the next gateway
            for (int j = i + 1; j <= nextGateway; j++)
            {
                forwardNeighbors.Add(j);
            }
            
            // Sort remaining forward nodes by distance
            forwardNeighbors.Sort((a, b) => Vector3.Distance(scatteredPositions[i], scatteredPositions[a]).CompareTo(Vector3.Distance(scatteredPositions[i], scatteredPositions[b])));
            
            // Branch 1 to 2 paths ahead
            int branchingPaths = Mathf.Min(Random.Range(1, 3), forwardNeighbors.Count);
            for (int k = 0; k < branchingPaths; k++)
            {
                int target = forwardNeighbors[k];
                if (!adjacencyList[i].Contains(target))
                {
                    connections.Add(new Vector2Int(i, target));
                    adjacencyList[i].Add(target);
                    adjacencyList[target].Add(i); // Bidirectional mapping
                }
            }
            
            // Failsafe: Guarantee connection reaching the gateway to prevent dead islands
            if (!adjacencyList[i].Contains(nextGateway) && nextGateway == i + 1)
            {
                connections.Add(new Vector2Int(i, nextGateway));
                adjacencyList[i].Add(nextGateway);
                adjacencyList[nextGateway].Add(i);
            }
        }

        // 5. Instantiate lines
        foreach (Vector2Int edge in connections)
        {
            GameObject lineObj = new GameObject($"MapLine_{edge.x}_{edge.y}");
            lineObj.transform.SetParent(mapNodeTransform);
            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            
            // Visual configuration
            lr.alignment = LineAlignment.TransformZ;
            lr.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            lr.numCornerVertices = 4;
            lr.numCapVertices = 4;
            if (pathLineMaterial != null) lr.material = pathLineMaterial;
            
            lr.startWidth = 0.5f;
            lr.endWidth = 0.5f;
            lr.useWorldSpace = true;
            
            // Position mapping
            lr.positionCount = 2;
            lr.SetPosition(0, scatteredPositions[edge.x] + new Vector3(0, lineVerticalOffset, 0));
            lr.SetPosition(1, scatteredPositions[edge.y] + new Vector3(0, lineVerticalOffset, 0));
            
            spawnedLines.Add(lineObj);
        }

        // 6. Instantiate node objects
        for (int i = 0; i < domainLevelSelection.levelList.Length; i++)
        {
            Vector3 finalPosition = scatteredPositions[i];
            GameObject newNode = Instantiate(mapNode, finalPosition, Quaternion.identity);
            
            // Assign the type calculated earlier
            NodeType nodeType = predefinedNodeTypes[i]; 

            MapNodeController nodeController = newNode.GetComponentInChildren<MapNodeController>();
            if (nodeController != null)
            {
                nodeController.type = nodeType;
                nodeController.nodeId = i;
                nodeController.mapGenerator = this;
            }

            spawnedNodes.Add(newNode);

            var enemySelection = newNode.GetComponent<EnemySelection>();
            enemySelection.enemyParty = domainLevelSelection.levelList[i].enemyPartyData;
            enemySelection.levelNumber = domainLevelSelection.levelList[i].levelNumber;
            
            // --- UPDATED: Pass the current index 'i' to evaluate difficulty ---
            MapData randomlySelectedMap = GetRandomMapForType(nodeType, i);
            if (randomlySelectedMap != null)
            {
                enemySelection.mapData = randomlySelectedMap;
            }
            else
            {
                // Fallback gracefully just in case a pool is completely empty
                enemySelection.mapData = domainLevelSelection.levelList[i].map;
            }

            // Make all nodes interactable globally, control state via exact IDs
            bool isCleared = gameSaveData.clearedNodesId != null && gameSaveData.clearedNodesId.Contains(i);

            if (isCleared) nodeController.SetCleared();
            else nodeController.SetUnlocked();

            UpdateNodeVisuals(newNode, isCleared);

            int visualStartNodeId = gameSaveData.currentNodeId;
            if (visualStartNodeId < 0 || visualStartNodeId >= domainLevelSelection.levelList.Length)
            {
                visualStartNodeId = highestUnlockedLevel;
            }

            if (i == visualStartNodeId)
            {
                currentMapNodeTransform = newNode.transform;
                currentNodeId = i; 
                UpdatePartyMemberVisuals(newNode);
            }

            nodePositions.Add(finalPosition);
        }
    }

    // --- UPDATED: Random Map Fetcher with Difficulty Support ---
    private MapData GetRandomMapForType(NodeType type, int nodeIndex)
    {
        List<MapData> pool = null;

        switch (type)
        {
            case NodeType.RegularBattle: pool = regularMaps; break;
            case NodeType.PuzzleBattle: pool = puzzleMaps; break;
            case NodeType.MinibossBattle: pool = minibossMaps; break;
            case NodeType.BossBattle: pool = bossMaps; break;
        }

        if (pool != null && pool.Count > 0)
        {
            if (enforceDifficultyProgression)
            {
                // 1. Determine target difficulty
                MapData.Difficulty targetDifficulty = MapData.Difficulty.Hard;
                if (nodeIndex <= maxEasyNodeIndex)
                {
                    targetDifficulty = MapData.Difficulty.Easy;
                }
                else if (nodeIndex <= maxMediumNodeIndex)
                {
                    targetDifficulty = MapData.Difficulty.Medium;
                }

                // 2. Filter pool based on difficulty
                List<MapData> filteredPool = pool.FindAll(m => m.difficultyLevel == targetDifficulty);

                // 3. Return from filtered pool if possible
                if (filteredPool.Count > 0)
                {
                    return filteredPool[Random.Range(0, filteredPool.Count)];
                }
                else
                {
                    Debug.LogWarning($"[MapGenerator] No MapData found for {type} with difficulty {targetDifficulty}. Falling back to any difficulty in the pool.");
                }
            }

            // Fallback: If difficulty is disabled or the specific specific difficulty pool was empty, 
            // pull randomly from the entire valid pool.
            return pool[Random.Range(0, pool.Count)];
        }
        
        Debug.LogWarning($"[MapGenerator] No MapData found in pool for {type}. Will fallback to default domain assignment.");
        return null;
    }

    public void MoveToNode(int targetId)
    {
        if (isMoving || currentNodeId == targetId) return;

        List<int> shortestPath = PathfindingBFS(currentNodeId, targetId);
        
        if (shortestPath != null && shortestPath.Count > 1) 
        {
            StartCoroutine(MovePartyRoutine(shortestPath));
        }
        else
        {
            // The path is blocked by a gateway or is unreachable. Provide visual feedback.
            TriggerShakePartyRoutine();
        }
    }

    // Making this public gives map nodes the ability to intentionally trigger a rejection shake!
    public void TriggerShakePartyRoutine()
    {
        StartCoroutine(ShakePartyRoutine());
    }

    private IEnumerator ShakePartyRoutine()
    {
        isMoving = true;
        Tween waitTween = null;

        for (int pIndex = 0; pIndex < spawnedPartyIcons.Count; pIndex++)
        {
            if (spawnedPartyIcons[pIndex] != null)
            {
                // Shake on the X/Z axis slightly to indicate "nope"
                waitTween = spawnedPartyIcons[pIndex].transform
                    .DOShakePosition(0.3f, new Vector3(0.5f, 0, 0.5f), 15, 90f, false, true);
            }
        }

        if (waitTween != null)
        {
            yield return waitTween.WaitForCompletion();
        }

        isMoving = false;
    }

    private List<int> PathfindingBFS(int start, int target)
    {
        Queue<int> pathQueue = new Queue<int>();
        Dictionary<int, int> parentMap = new Dictionary<int, int>();
        
        pathQueue.Enqueue(start);
        parentMap[start] = -1;

        while (pathQueue.Count > 0)
        {
            int current = pathQueue.Dequeue();
            
            if (current == target) break;

            foreach (int neighbor in adjacencyList[current])
            {
                if (!parentMap.ContainsKey(neighbor))
                {
                    MapNodeController currentController = spawnedNodes[current].GetComponentInChildren<MapNodeController>();
                    MapNodeController neighborController = spawnedNodes[neighbor].GetComponentInChildren<MapNodeController>();

                    bool currentIsUnclearedGateway = currentController != null && 
                        (currentController.type == NodeType.MinibossBattle || currentController.type == NodeType.BossBattle) && 
                        currentController.currentLockStatus != MapNodeController.LockStatus.levelCleared;

                    bool neighborIsUnclearedGateway = neighborController != null && 
                        (neighborController.type == NodeType.MinibossBattle || neighborController.type == NodeType.BossBattle) && 
                        neighborController.currentLockStatus != MapNodeController.LockStatus.levelCleared;

                    // 1. Strict Barrier: You cannot move FORWARD from an uncleared Gateway to any other node. 
                    // (But retreating BACKWARD to a lower ID is allowed)
                    if (currentIsUnclearedGateway && neighbor > current)
                    {
                        continue;
                    }

                    // 2. Choke Point check: You cannot path THROUGH an uncleared Gateway to reach something else.
                    if (neighborIsUnclearedGateway && neighbor != target && neighbor > current)
                    {
                        continue; 
                    }

                    parentMap[neighbor] = current;
                    pathQueue.Enqueue(neighbor);
                }
            }
        }

        if (!parentMap.ContainsKey(target)) return null; // Path blocked / not found.

        List<int> calculatedPath = new List<int>();
        int backtrackNode = target;

        while (backtrackNode != -1)
        {
            calculatedPath.Add(backtrackNode);
            backtrackNode = parentMap[backtrackNode];
        }
        
        calculatedPath.Reverse();
        return calculatedPath;
    }

    private IEnumerator MovePartyRoutine(List<int> path)
    {
        isMoving = true;
        
        GameStatsManager gameStatsManager = FindAnyObjectByType<GameStatsManager>();

        for (int i = 1; i < path.Count; i++) // Starts from 1 assuming 0 is actual Start Position.
        {
            int nextNode = path[i];
            Vector3 targetPosition = nodePositions[nextNode];
            
            float horizontalOffset = 2; 
            float startOffset = -(partyMemberIcons.Length - 1) * horizontalOffset * 0.5f; 

            Tween waitTween = null;

            for (int pIndex = 0; pIndex < spawnedPartyIcons.Count; pIndex++)
            {
                if (spawnedPartyIcons[pIndex] != null)
                {
                    Vector3 destOffsetPosition = new Vector3(startOffset + horizontalOffset * pIndex, 0, iconZOffset);

                    // Fire independent DOTween commands for each figure parallelly
                    waitTween = spawnedPartyIcons[pIndex].transform
                        .DOMove(targetPosition + destOffsetPosition, 0.4f)
                        .SetEase(Ease.InOutSine);
                }
            }

            // Wait for the final character's animation (which matches the others) to finish stepping.
            if (waitTween != null)
            {
                yield return waitTween.WaitForCompletion();
            }
            
            currentNodeId = nextNode;
            
            if (gameStatsManager != null)
            {
                gameStatsManager.SaveCurrentNodeId(currentNodeId);
            }
        }

        isMoving = false;
    }

    private void UpdatePartyMemberVisuals(GameObject mapNode)
    {
        Vector3 partyMemberIconPosition = mapNode.transform.position + new Vector3(0, 0, iconZOffset);
        float horizontalOffset = 2; 
        float startOffset = -(partyMemberIcons.Length - 1) * horizontalOffset * 0.5f; 

        for (int j = 0; j < partyMemberIcons.Length; j++)
        {
            Vector3 offsetPosition = new Vector3(startOffset + horizontalOffset * j, 0, 0);
            
            // 1. Create an empty wrapper GameObject to act as our uncontested move target
            GameObject iconWrapper = new GameObject($"PartyIconWrapper_{j}");
            iconWrapper.transform.position = partyMemberIconPosition + offsetPosition;

            // 2. Instantiate the prefab DIRECTLY as a local child. 
            // Passing 'false' prevents World-to-Local conversion mathematical jumps when the Animator activates.
            GameObject newIcon = Instantiate(partyMemberIcons[j], iconWrapper.transform, false);

            // 3. Defensively disable root motion so Animator curves only govern local graphical bounce/sway, not physics.
            Animator[] animators = newIcon.GetComponentsInChildren<Animator>();
            foreach (var anim in animators)
            {
                anim.applyRootMotion = false;
            }
            
            // 4. Track the wrapper so DOTween moves it instead of the animated child
            spawnedPartyIcons.Add(iconWrapper);
        }
    }

    private void UnlockLevel(GameObject mapNode)
    {
        mapNode.GetComponentInChildren<MapNodeController>().currentLockStatus = MapNodeController.LockStatus.levelUnlocked;
    }

    private void UpdateNodeVisuals(GameObject mapNode, bool isCleared = false)
    {
        MapNodeController nodeController = mapNode.GetComponentInChildren<MapNodeController>();
        Color color = nodeController != null ? GetNodeTypeColor(nodeController.type) : Color.white;
        
        if (isCleared)
        {
            color = Color.gray; // Visually indicate the node has been cleared
        }
        
        mapNode.GetComponentInChildren<MeshRenderer>().material.color = color;
    }

    private Color GetNodeTypeColor(NodeType type)
    {
        switch (type)
        {
            case NodeType.RegularBattle: return Color.white;
            case NodeType.PuzzleBattle: return Color.blue;
            case NodeType.MinibossBattle: return Color.yellow;
            case NodeType.BossBattle: return Color.red;
            default: return Color.white;
        }
    }

    private NodeType GenerateNodeType(int nodeIndex, int totalNodes)
    {
        // Require the config to be set, otherwise fallback to regular nodes.
        if (config == null) return NodeType.RegularBattle;

        // 1. Boss Battle can only spawn as the last node.
        if (nodeIndex == totalNodes - 1)
        {
            return NodeType.BossBattle;
        }

        // 2. Evaluate thresholds for other special nodes.
        float currentPuzzleWeight = (nodeIndex >= config.puzzleBattleThreshold) ? config.puzzleBattleWeight : 0f;
        float currentMinibossWeight = (nodeIndex >= config.minibossBattleThreshold) ? config.minibossBattleWeight : 0f;

        // 3. Calculate total valid weights for this specific index.
        float totalWeight = config.regularBattleWeight + currentPuzzleWeight + currentMinibossWeight;

        // Fallback safety
        if (totalWeight <= 0f) return NodeType.RegularBattle;

        float randomVal = Random.Range(0, totalWeight);

        if (randomVal < config.regularBattleWeight) return NodeType.RegularBattle;
        randomVal -= config.regularBattleWeight;

        if (randomVal < currentPuzzleWeight) return NodeType.PuzzleBattle;
        
        return NodeType.MinibossBattle;
    }

    private void RegenerateMap()
    {
        ClearMap();
        GenerateLevel(currentDomain);
    }

    private void ClearMap()
    {
        foreach (var node in spawnedNodes)
        {
            if (node != null) Destroy(node);
        }
        spawnedNodes.Clear();

        foreach (var icon in spawnedPartyIcons)
        {
            if (icon != null) Destroy(icon);
        }
        spawnedPartyIcons.Clear();
        
        foreach (var line in spawnedLines)
        {
            if (line != null) Destroy(line);
        }
        spawnedLines.Clear();

        nodePositions.Clear();
        adjacencyList.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        if (mapNodeTransform != null && config != null)
        {
            Gizmos.color = new Color(0, 1f, 1f, 0.5f);
            Vector3 center = mapNodeTransform.position + new Vector3(config.mapWidth / 2f, 0, 0);
            Vector3 size = new Vector3(config.mapWidth, 0.1f, config.mapDepth);
            Gizmos.DrawWireCube(center, size);
        }
    }
}