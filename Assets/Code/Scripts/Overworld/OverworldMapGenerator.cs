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
    public List<Domain> domains = new List<Domain>(); // Domains are a set of different Levels.
    public GameObject mapNode;
    public Transform mapNodeTransform;

    [Header("Configuration")]
    // Welcome! TomodachiPod here.
    // This is the most important file concerning the Map Overworld! Here you will configure the way the map generation works.
    // Basically from here you can shape the experience difficulty and dictate the pace of the progression through levels
    // in terms of difficulty and challenge types distribution.
    [Tooltip("Please create a MapConfig Scriptable Object and assign it here, this will dictate rules and weights.")]
    public MapGenerationConfig config;

    // Think of Maps as Lego Sets themes: there are different types: Regular (normal skirmishes), Puzzle (maps which have some Zelda-like elements),
    // MiniBoss/Boss Maps (where Player must confront MiniBoss/Boss). Mind that this is an arbitrary design distinction, as what really matters
    // is the content that was designed itself. So for each Lego Set theme (like, pirates) there'll be many subsets (like, treasure cove, pirate ship, island, etc).
    // So, as a designer, please make sure that you're adding the correct maps. Example: if you design a Puzzle Map, that'll typically contain
    // switches, chests and so on, you will need to add it to the Puzzle Maps pool. This is important because the OverworldMap generator
    // will distribute nodes along the entire length of the map.
    // Note: in the current build, only one map is assigned to each pool. So, the system is there, but you can't really see randomization in action.
    // However, as a designer, you will create more maps, add them to the pools, and this class will randomly pickem up and offer them to the player
    // based on the following distribution rules. (see Difficulty Progression.)
 
    [Header("Map Data Pools")]
    [Tooltip("Maps to randomly select from based on the generated Node Type.")]
    public List<MapData> regularMaps = new List<MapData>();
    public List<MapData> puzzleMaps = new List<MapData>();
    public List<MapData> minibossMaps = new List<MapData>();
    public List<MapData> bossMaps = new List<MapData>();

    // Difficulty Progression.
    // This set of rule dictates the distribution of maps based on their difficulty level. Mind that each MapData has a Difficulty Enum.
    // I guess this is pretty straightforward, but let's make an example. Now, the Puzzle Lego Set will contain Treasure Cove (Easy), Island (Medium), Tavern (Medium), Ship (Hard).
    // We want to dictate what exactly during the journey in a Domain, the Player will face.
    // With default config, this means that up until Node 3, if the Player enters a Puzzle Node, it will be either Treasure Cove or Island.
    // Up until 7, it will be either Island or Tavern. From 8 on, it will be only Ship.
    // Note for me: verify edge cases: what happens if there's Medium Only rule enforced, and there are no Medium tier maps?

    [Header("Difficulty Progression")]
    public bool enforceDifficultyProgression = true;
    [Tooltip("Nodes up to this index will exclusively pull Easy maps.")]
    public int maxEasyNodeIndex = 3;
    [Tooltip("Nodes after Easy and up to this index will pull Medium maps. Anything higher pulls Hard maps.")]
    public int maxMediumNodeIndex = 7;

    [Header("Visuals & UI")]
    [Tooltip("Assign a material for the line connecting Map Nodes.")]
    public Material pathLineMaterial;
    [Tooltip("Change this offset to avoid the lide compenetraing with the level floor")]
    public float lineVerticalOffset = 0.2f; 
    
    public GameObject[] partyMemberIcons;
    public float iconZOffset = 1f;

    [Header("Debug & Runtime Edit")]
    [Tooltip("Toggle this to regenerate map at runtime.")]
    public bool autoUpdateInPlayMode = true;

    [Tooltip("Assign a MapData here to teleport directly to the boss battle with it by pressing B.")]
    public MapData debugBossMapData;

    [Tooltip("If active, nodes listed in debugNodeMapOverrides will ignore the random pool and use the assigned MapData instead.")]
    public bool debugMapDataPopulationIsActive = false;

    // This debug is relevant for the current build (04082026), as I want to keep the randomization but being able to cherry pick a map.
    // In particular, I need to insert a Puzzle Map before the Boss Battle, so that the Player can collect a Boss Key.

    [System.Serializable]
    public class DebugNodeMapOverride
    {
        public int nodeIndex;
        public MapData mapData;
    }

    public List<DebugNodeMapOverride> debugNodeMapOverrides = new List<DebugNodeMapOverride>();

    [HideInInspector] public Transform currentMapNodeTransform;
    [HideInInspector] public int currentNodeId; 

    private List<Vector3> nodePositions = new List<Vector3>();
    private int currentDomainId = 0;

    private Domain currentDomain;
    public List<GameObject> spawnedNodes { get; private set; } = new List<GameObject>();
    private List<GameObject> spawnedPartyIcons = new List<GameObject>();
    private List<GameObject> spawnedLines = new List<GameObject>();
    private bool needsRegeneration = false;
    
    private Dictionary<int, List<int>> adjacencyList = new Dictionary<int, List<int>>();
    private bool isMoving = false;

    private float lastMapWidth;
    private float lastMapDepth;
    private float lastMinDistance;
    private int lastSeed;
    
    // Tracking config weights and thresholds.
    private float lastRegularWeight;
    private float lastPuzzleWeight;
    private float lastMinibossWeight;
    private float lastBossWeight;
    private int lastPuzzleThreshold;
    private int lastMinibossThreshold;

    // Hold a runtime copy to avoid permanently modifying the actual SO in the Editor.
    private MapGenerationConfig runtimeConfig;

    public Domain CurrentDomain => currentDomain;

    public void GenerateLevel(Domain domainLevelSelection)
    {
        if (config == null)
        {
            Debug.LogError("MapGenerationConfig is missing! Please assign one in the inspector.");
            return;
        }

        // Initialize tracking variables for automatic runtime regeneration.
        lastMapWidth = config.mapWidth;
        lastMapDepth = config.mapDepth;
        lastMinDistance = config.minDistanceApart;
        lastSeed = config.randomSeed;
        lastRegularWeight = config.regularBattleWeight;
        lastPuzzleWeight = config.puzzleBattleWeight;
        lastMinibossWeight = config.minibossBattleWeight;
        lastBossWeight = config.bossBattleWeight;
        lastPuzzleThreshold = config.puzzleBattleThreshold;
        lastMinibossThreshold = config.minibossBattleThreshold;

        currentDomain = domainLevelSelection; 
        GameSaveData gameSaveData = SaveStateManager.saveData; // Retrieve the Player's progression (aka, how many nodes they have completed).
        int highestUnlockedLevel = gameSaveData.highestUnlockedLevel;

        // Create a temporary clone of the config for this generation run.
        runtimeConfig = Instantiate(config);

        // If either randomization rule is active, we rely on a persistent Run Seed
        if (runtimeConfig.randomizeSeedOnGeneration || runtimeConfig.fullyRandomizeRules)
        {
            // If starting a fresh run, generate a new persistent seed and save it!
            if (gameSaveData.runSeed == 0)
            {
                gameSaveData.runSeed = Random.Range(1, int.MaxValue);
                SaveStateManager.SaveGame(gameSaveData); 
            }

            // Initialize Unity's randomizer with the saved Run Seed
            Random.InitState(gameSaveData.runSeed);

            // Deterministically randomize the rules if Roguelike Mode is active. Seed is fixes, ranged will yield same values
            // anytime Player loads this save file.
            if (runtimeConfig.fullyRandomizeRules)
            {
                runtimeConfig.mapWidth = Random.Range(runtimeConfig.mapWidthRange.x, runtimeConfig.mapWidthRange.y);
                runtimeConfig.minDistanceApart = Random.Range(runtimeConfig.minDistanceRange.x, runtimeConfig.minDistanceRange.y);

                runtimeConfig.regularBattleWeight = Random.Range(runtimeConfig.regularWeightRange.x, runtimeConfig.regularWeightRange.y);
                runtimeConfig.puzzleBattleWeight = Random.Range(runtimeConfig.puzzleWeightRange.x, runtimeConfig.puzzleWeightRange.y);
                runtimeConfig.minibossBattleWeight = Random.Range(runtimeConfig.minibossWeightRange.x, runtimeConfig.minibossWeightRange.y);

                runtimeConfig.puzzleBattleThreshold = Random.Range(runtimeConfig.puzzleThresholdRange.x, runtimeConfig.puzzleThresholdRange.y + 1);
                runtimeConfig.minibossBattleThreshold = Random.Range(runtimeConfig.minibossThresholdRange.x, runtimeConfig.minibossThresholdRange.y + 1);
            }
            
            // Re-seed one more time right before node layout generation, as complete safety measure.
            Random.InitState(gameSaveData.runSeed);
        }
        else
        {
            // If NOT doing a randomized run, use the standard editor config seed.
            Random.InitState(runtimeConfig.randomSeed);
        }

        Vector3 initialPosition = mapNodeTransform.position;
        List<Vector3> scatteredPositions = new List<Vector3>();
        adjacencyList.Clear();

        // Scatter nodes avoiding overlapping (visual-wise).
        for (int i = 0; i < domainLevelSelection.levelList.Length; i++)
        {
            Vector3 testPosition = Vector3.zero;
            bool isValid = false;
            int attempt = 0;

            while (!isValid && attempt < 500)
            {
                attempt++;
                // Make sure to use 'runtimeConfig' instead of 'config' in generation maths.
                float randomX = Random.Range(0, runtimeConfig.mapWidth);
                float randomZ = Random.Range(-runtimeConfig.mapDepth / 2f, runtimeConfig.mapDepth / 2f);
                testPosition = initialPosition + new Vector3(randomX, 0, randomZ);

                isValid = true;
                
                foreach (Vector3 pos in scatteredPositions)
                {
                    if (Vector3.Distance(testPosition, pos) < runtimeConfig.minDistanceApart)
                    {
                        isValid = false;
                        break;
                    }
                }
            }

            scatteredPositions.Add(testPosition);
            adjacencyList[i] = new List<int>(); 
        }

        // Sort from left to right to build an advancing mesh
        scatteredPositions.Sort((a, b) => a.x.CompareTo(b.x));
        
        // Pre-generate node types to identify gateways (choke points) early.
        // TomodachiPod here! :D Please remember that gateways are special nodes that prevent the Player
        // To progress further unless they've completed them. It makes sense for a gateway to be a Miniboss fight.
        
        NodeType[] predefinedNodeTypes = new NodeType[scatteredPositions.Count];
        for (int i = 0; i < scatteredPositions.Count; i++)
        {
            predefinedNodeTypes[i] = GenerateNodeType(i, scatteredPositions.Count);
        }

        // Determine mesh connections (multi-path) restricted by Gateways.
        List<Vector2Int> connections = new List<Vector2Int>();
        for (int i = 0; i < scatteredPositions.Count - 1; i++) // Skip the last node since it has no forward neighbors.
        {
            // Find the closest gateway node ahead of the current node.
            int nextGateway = scatteredPositions.Count - 1; // Default to the very last node if no gateway is found.
            for (int g = i + 1; g < scatteredPositions.Count; g++)
            {
                if (predefinedNodeTypes[g] == NodeType.MinibossBattle || predefinedNodeTypes[g] == NodeType.BossBattle)
                {
                    nextGateway = g;
                    break;
                }
            }

            List<int> forwardNeighbors = new List<int>();
            // You can only connect to nodes up to the next gateway.
            for (int j = i + 1; j <= nextGateway; j++)
            {
                forwardNeighbors.Add(j);
            }
            
            // Sort remaining forward nodes by distance.
            forwardNeighbors.Sort((a, b) => Vector3.Distance(scatteredPositions[i], scatteredPositions[a]).CompareTo(Vector3.Distance(scatteredPositions[i], scatteredPositions[b])));
            
            // Branch 1 to 2 paths ahead.
            int branchingPaths = Mathf.Min(Random.Range(1, 3), forwardNeighbors.Count);
            for (int k = 0; k < branchingPaths; k++)
            {
                int target = forwardNeighbors[k];
                if (!adjacencyList[i].Contains(target))
                {
                    connections.Add(new Vector2Int(i, target));
                    adjacencyList[i].Add(target);
                    adjacencyList[target].Add(i); // Bidirectional mapping.
                }
            }
            
            // Failsafe: Guarantee connection reaching the gateway to prevent dead islands.
            if (!adjacencyList[i].Contains(nextGateway) && nextGateway == i + 1)
            {
                connections.Add(new Vector2Int(i, nextGateway));
                adjacencyList[i].Add(nextGateway);
                adjacencyList[nextGateway].Add(i);
            }
        }

        // Instantiate lines.
        foreach (Vector2Int edge in connections)
        {
            GameObject lineObj = new GameObject($"MapLine_{edge.x}_{edge.y}");
            lineObj.transform.SetParent(mapNodeTransform);
            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            
            // Visual configuration.
            lr.alignment = LineAlignment.TransformZ;
            lr.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            lr.numCornerVertices = 4;
            lr.numCapVertices = 4;
            if (pathLineMaterial != null) lr.material = pathLineMaterial;
            
            lr.startWidth = 0.5f;
            lr.endWidth = 0.5f;
            lr.useWorldSpace = true;
            
            // Position mapping.
            lr.positionCount = 2;
            lr.SetPosition(0, scatteredPositions[edge.x] + new Vector3(0, lineVerticalOffset, 0));
            lr.SetPosition(1, scatteredPositions[edge.y] + new Vector3(0, lineVerticalOffset, 0));
            
            spawnedLines.Add(lineObj);
        }

        // Instantiate node objects.
        for (int i = 0; i < domainLevelSelection.levelList.Length; i++)
        {
            Vector3 finalPosition = scatteredPositions[i];
            GameObject newNode = Instantiate(mapNode, finalPosition, Quaternion.identity);
            
            // Assign the type calculated earlier.
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
            
            // Pass the current index 'i' to evaluate difficulty.
            MapData randomlySelectedMap = GetRandomMapForType(nodeType, i);
            if (randomlySelectedMap != null)
            {
                enemySelection.mapData = randomlySelectedMap;
            }
            else
            {
                // Fallback just in case a pool is completely empty.
                enemySelection.mapData = domainLevelSelection.levelList[i].map;
            }

            // Debug cherrypick: forces this node to use a specific MapData regardless of pool selection.
            MapData debugOverrideMap = GetDebugMapOverrideForNode(i);
            if (debugOverrideMap != null)
            {
                enemySelection.mapData = debugOverrideMap;
                Debug.Log($"[DEBUG OVERRIDE] Node {i} forced to use MapData '{debugOverrideMap.name}'.");
            }

            // Make all nodes interactable globally, control state via exact IDs.
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

    private MapData GetDebugMapOverrideForNode(int nodeIndex)
    {
        if (!debugMapDataPopulationIsActive) return null;

        foreach (var entry in debugNodeMapOverrides)
        {
            if (entry.nodeIndex == nodeIndex) return entry.mapData;
        }

        return null;
    }

    // Random Map Fetcher with Difficulty Support.
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
                // Determine target difficulty
                MapData.Difficulty targetDifficulty = MapData.Difficulty.Hard;
                if (nodeIndex <= maxEasyNodeIndex)
                {
                    targetDifficulty = MapData.Difficulty.Easy;
                }
                else if (nodeIndex <= maxMediumNodeIndex)
                {
                    targetDifficulty = MapData.Difficulty.Medium;
                }

                // Filter pool based on difficulty.
                List<MapData> filteredPool = pool.FindAll(m => m.difficultyLevel == targetDifficulty);

                // Return from filtered pool if possible.
                if (filteredPool.Count > 0)
                {
                    return filteredPool[Random.Range(0, filteredPool.Count)];
                }
                else
                {
                    Debug.LogWarning($"[MapGenerator] No MapData found for {type} with difficulty {targetDifficulty}. Falling back to any difficulty in the pool.");
                }
            }

            // Fallback: If difficulty is disabled or the specific specific difficulty pool was empty, pull randomly from the entire valid pool.
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

                    bool currentIsGateway = currentController != null && 
                        (currentController.type == NodeType.MinibossBattle || currentController.type == NodeType.BossBattle);

                    bool currentIsUnclearedGateway = currentIsGateway && 
                        currentController.currentLockStatus != MapNodeController.LockStatus.levelCleared;

                    bool neighborIsUnclearedGateway = neighborController != null && 
                        (neighborController.type == NodeType.MinibossBattle || neighborController.type == NodeType.BossBattle) && 
                        neighborController.currentLockStatus != MapNodeController.LockStatus.levelCleared;

                    // Strict Barrier: You cannot move FORWARD from a Gateway to any other node
                    // if the Gateway is uncleared OR if the previous node completion requirement isn't met.
                    if (currentIsGateway && neighbor > current)
                    {
                        if (currentIsUnclearedGateway)
                        {
                            continue; // Block forward movement if gateway itself isn't beaten yet (Always mandatory)
                        }

                        if (runtimeConfig != null && runtimeConfig.enforceChokepointProgressionRule)
                        {
                            // Calculate how many nodes exist from start up to (and including) this gateway.
                            int totalNodesUpToGateway = current + 1; 
                            
                            // Calculate required amount based on the percentage.
                            int requiredClearedCount = Mathf.CeilToInt(totalNodesUpToGateway * (runtimeConfig.chokepointCompletionPercentageRequired / 100f));
                            
                            int clearedCount = 0;
                            // Count how many nodes in this chunk have actually been cleared.
                            for (int i = 0; i <= current; i++)
                            {
                                var prevController = spawnedNodes[i].GetComponentInChildren<MapNodeController>();
                                if (prevController != null && prevController.currentLockStatus == MapNodeController.LockStatus.levelCleared)
                                {
                                    clearedCount++;
                                }
                            }

                            if (clearedCount < requiredClearedCount)
                            {
                                continue; // Block forward movement due to not meeting the completion percentage
                            }
                        }
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
        RoamingDeityController deityController = FindAnyObjectByType<RoamingDeityController>();

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

            // Increase Calendar Day.
            if (OverworldMapManager.Instance != null && OverworldMapManager.Instance.CalendarController != null)
            {
                OverworldMapManager.Instance.CalendarController.IncreaseDaysCounter(1);
            }

            // --- TRIGGER DEITY MOVEMENT ---
            if (deityController != null)
            {
                deityController.OnPlayerMoved(currentNodeId);
                
                // Optional: add a tiny visual delay if you want the deity to move visibly right after the player stats
                yield return new WaitForSeconds(0.6f); 
            }
        }

        isMoving = false;
    }

    private void UpdatePartyMemberVisuals(GameObject mapNode)
    {
        // 0. FETCH ACTIVE PARTY FROM GAMEMANAGER
        List<GameObject> dynamicPartyIcons = new List<GameObject>();
        if (GameManager.Instance != null && GameManager.Instance.playerPartyMembers.Count > 0)
        {
            // Only spawn up to the active party cap (3)
            int limit = Mathf.Min(GameManager.MaxActivePartySize, GameManager.Instance.playerPartyMembers.Count);
            for (int i = 0; i < limit; i++)
            {
                dynamicPartyIcons.Add(GameManager.Instance.playerPartyMembers[i].gameObject);
            }
        }
        else
        {
            // Fallback to inspector array if playing scene directly without bootup
            dynamicPartyIcons.AddRange(partyMemberIcons);
        }

        Vector3 partyMemberIconPosition = mapNode.transform.position + new Vector3(0, 0, iconZOffset);
        float horizontalOffset = 2; 
        float startOffset = -(dynamicPartyIcons.Count - 1) * horizontalOffset * 0.5f; 

        // Cache the required Layer integer natively to avoid string lookups in the loop
        int unitMapIconLayer = LayerMask.NameToLayer("UnitMapIcon");

        for (int j = 0; j < dynamicPartyIcons.Count; j++)
        {
            if (dynamicPartyIcons[j] == null) continue;

            Vector3 offsetPosition = new Vector3(startOffset + horizontalOffset * j, 0, 0);
            
            // 1. Create an empty wrapper GameObject to act as our uncontested move target
            GameObject iconWrapper = new GameObject($"PartyIconWrapper_{dynamicPartyIcons[j].name}");
            iconWrapper.transform.position = partyMemberIconPosition + offsetPosition;

            // 2. Instantiate the prefab DIRECTLY as a local child. 
            GameObject newIcon = Instantiate(dynamicPartyIcons[j], iconWrapper.transform, false);

            // Optional cleanup: Ensure the prefab instance behaves strictly as an UI prop!
            var unitScript = newIcon.GetComponent<Unit>();
            if (unitScript != null) Destroy(unitScript);

            // 3. Defensively disable root motion so Animator curves only govern local graphical bounce/sway
            Animator[] animators = newIcon.GetComponentsInChildren<Animator>();
            foreach (var anim in animators)
            {
                anim.applyRootMotion = false;
            }
            
            // 4. --- NEW: FORCE LAYER TO 'UnitMapIcon' FOR VISIBILITY ---
            if (unitMapIconLayer != -1) // Ensure the layer actually exists in the project
            {
                newIcon.layer = unitMapIconLayer;
                // Recursively set all child objects (e.g., meshes, weapons, models) to the layer
                foreach (Transform child in newIcon.GetComponentsInChildren<Transform>(true))
                {
                    child.gameObject.layer = unitMapIconLayer;
                }
            }
            else
            {
                Debug.LogWarning("[OverworldMapGenerator] Layer 'UnitMapIcon' does not exist in your project settings!");
            }
            
            // 5. Track the wrapper so DOTween moves it instead of the animated child
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
        if (runtimeConfig == null) return NodeType.RegularBattle;

        // 1. Boss Battle can only spawn as the last node.
        if (nodeIndex == totalNodes - 1)
        {
            return NodeType.BossBattle;
        }

        // 2. Evaluate thresholds for other special nodes.
        float currentPuzzleWeight = (nodeIndex >= runtimeConfig.puzzleBattleThreshold) ? runtimeConfig.puzzleBattleWeight : 0f;
        float currentMinibossWeight = (nodeIndex >= runtimeConfig.minibossBattleThreshold) ? runtimeConfig.minibossBattleWeight : 0f;

        // 3. Calculate total valid weights for this specific index.
        float totalWeight = runtimeConfig.regularBattleWeight + currentPuzzleWeight + currentMinibossWeight;

        // Fallback safety
        if (totalWeight <= 0f) return NodeType.RegularBattle;

        float randomVal = Random.Range(0, totalWeight);

        if (randomVal < runtimeConfig.regularBattleWeight) return NodeType.RegularBattle;
        randomVal -= runtimeConfig.regularBattleWeight;

        if (randomVal < currentPuzzleWeight) return NodeType.PuzzleBattle;
        
        return NodeType.MinibossBattle;
    }

    private void RegenerateMap()
    {
        ClearMap();
        GenerateLevel(currentDomain);
    }

    private void Update()
    {
        if (!Application.isPlaying || !autoUpdateInPlayMode || config == null || currentDomain == null) return;

        bool hasChanged = false;

        if (config.mapWidth != lastMapWidth || config.mapDepth != lastMapDepth || config.minDistanceApart != lastMinDistance || config.randomSeed != lastSeed)
            hasChanged = true;

        if (config.regularBattleWeight != lastRegularWeight || config.puzzleBattleWeight != lastPuzzleWeight || 
            config.minibossBattleWeight != lastMinibossWeight || config.bossBattleWeight != lastBossWeight)
            hasChanged = true;

        if (config.puzzleBattleThreshold != lastPuzzleThreshold || config.minibossBattleThreshold != lastMinibossThreshold)
            hasChanged = true;

        if (hasChanged)
        {
            RegenerateMap();
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            DebugTeleportToBossBattle();
        }
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

    public void DebugTeleportToBossBattle()
    {
        if (currentDomain == null)
        {
            Debug.LogError("Current Domain is not set. Please generate a map first or ensure a domain is active.");
            return;
        }

        if (debugBossMapData == null)
        {
            Debug.LogError("Debug Boss Map Data is not assigned. Please assign it in the Inspector.");
            return;
        }

        // --- Main Teleport Logic ---
        // Temporarily override the save data to force the player's spawn on the last node.
        GameSaveData gameSaveData = SaveStateManager.saveData;
        int bossNodeId = currentDomain.levelList.Length - 1;

        if (bossNodeId < 0)
        {
            Debug.LogError("Cannot teleport: The selected domain has no levels.");
            return;
        }
        
        gameSaveData.currentNodeId = bossNodeId;

        // Regenerate the map. It will now use the overridden start position.
        ClearMap();
        GenerateLevel(currentDomain);
        
        // The generator has now placed the player at the boss node.
        // Now, we inject the specific debug map data into that node.
        if (spawnedNodes.Count > bossNodeId && spawnedNodes[bossNodeId] != null)
        {
            EnemySelection bossEnemySelection = spawnedNodes[bossNodeId].GetComponent<EnemySelection>();
            if (bossEnemySelection != null)
            {
                bossEnemySelection.mapData = debugBossMapData;
            }
        }
        
        // Optional: Persist this change to the save file.
        GameStatsManager statsManager = FindObjectOfType<GameStatsManager>();
        if (statsManager != null)
        {
            statsManager.SaveCurrentNodeId(bossNodeId);
        }

        Debug.Log($"Successfully teleported to Boss Battle (Node {bossNodeId}).");
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