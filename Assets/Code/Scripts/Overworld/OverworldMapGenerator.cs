using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // Import DOTween 

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
    [Tooltip("Assegna qui la tua configurazione ScriptableObject")]
    public MapGenerationConfig config;

    [Header("Node Distribution Weights")]
    [Range(0, 100)] public float regularBattleWeight = 70f;
    [Range(0, 100)] public float puzzleBattleWeight = 20f;
    [Range(0, 100)] public float minibossBattleWeight = 10f;
    [Range(0, 100)] public float bossBattleWeight = 0f; // Typically placed deliberately

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
        }
    }

    private bool HasConfigChanged()
    {
        if (config == null) return false;

        return lastMapWidth != config.mapWidth ||
               lastMapDepth != config.mapDepth ||
               lastMinDistance != config.minDistanceApart ||
               lastSeed != config.randomSeed;
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

        // 3. Determine mesh connections (multi-path)
        List<Vector2Int> connections = new List<Vector2Int>();
        for (int i = 0; i < scatteredPositions.Count; i++)
        {
            List<int> forwardNeighbors = new List<int>();
            for (int j = i + 1; j < scatteredPositions.Count; j++)
            {
                forwardNeighbors.Add(j);
            }
            
            // Sort remaining forward nodes by distance
            forwardNeighbors.Sort((a, b) => Vector3.Distance(scatteredPositions[i], scatteredPositions[a]).CompareTo(Vector3.Distance(scatteredPositions[i], scatteredPositions[b])));
            
            // Randomly branch 1 to 2 paths ahead towards the nearest nodes
            int branchingPaths = Mathf.Min(Random.Range(1, 3), forwardNeighbors.Count);
            for (int k = 0; k < branchingPaths; k++)
            {
                connections.Add(new Vector2Int(i, forwardNeighbors[k]));
                adjacencyList[i].Add(forwardNeighbors[k]);
                adjacencyList[forwardNeighbors[k]].Add(i); // Make paths bidirectional.
            }
        }

        // 4. Instantiate lines
        foreach (Vector2Int edge in connections)
        {
            GameObject lineObj = new GameObject($"MapLine_{edge.x}_{edge.y}");
            lineObj.transform.SetParent(mapNodeTransform);
            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            
            lr.alignment = LineAlignment.TransformZ;
            lr.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            lr.numCornerVertices = 4;
            lr.numCapVertices = 4;
            if (pathLineMaterial != null) lr.material = pathLineMaterial;
            
            lr.startWidth = 0.5f;
            lr.endWidth = 0.5f;
            lr.useWorldSpace = true;
            
            lr.positionCount = 2;
            lr.SetPosition(0, scatteredPositions[edge.x] + new Vector3(0, lineVerticalOffset, 0));
            lr.SetPosition(1, scatteredPositions[edge.y] + new Vector3(0, lineVerticalOffset, 0));
            
            spawnedLines.Add(lineObj);
        }

        // 5. Instantiate node objects
        for (int i = 0; i < domainLevelSelection.levelList.Length; i++)
        {
            Vector3 finalPosition = scatteredPositions[i];

            GameObject newNode = Instantiate(mapNode, finalPosition, Quaternion.identity);
            NodeType nodeType = GenerateNodeType();

            MapNodeController nodeController = newNode.GetComponentInChildren<MapNodeController>();
            if (nodeController != null)
            {
                nodeController.type = nodeType;
                nodeController.nodeId = i;
                nodeController.mapGenerator = this;
            }

            spawnedNodes.Add(newNode);

            newNode.GetComponent<EnemySelection>().enemyParty = domainLevelSelection.levelList[i].enemyPartyData;
            newNode.GetComponent<EnemySelection>().levelNumber = domainLevelSelection.levelList[i].levelNumber;
            newNode.GetComponent<EnemySelection>().mapData = domainLevelSelection.levelList[i].map;

            // Make all nodes interactable globally, but control state via IDs.
            UpdateNodeVisuals(newNode);
            UnlockLevel(newNode);

            // Determine if this is the start position (fallback to highest unlocked level dynamically).
            int visualStartNodeId = gameSaveData.currentNodeId;
            if (visualStartNodeId < 0 || visualStartNodeId >= domainLevelSelection.levelList.Length)
            {
                visualStartNodeId = highestUnlockedLevel;
            }

            // Keep the party member visual on the relevant progression node
            if (i == visualStartNodeId)
            {
                currentMapNodeTransform = newNode.transform;
                currentNodeId = i; 
                UpdatePartyMemberVisuals(newNode);
            }

            nodePositions.Add(finalPosition);
        }
    }

    public void MoveToNode(int targetId)
    {
        if (isMoving || currentNodeId == targetId) return;

        List<int> shortestPath = PathfindingBFS(currentNodeId, targetId);
        
        if (shortestPath != null && shortestPath.Count > 1) 
        {
            StartCoroutine(MovePartyRoutine(shortestPath));
        }
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
                    parentMap[neighbor] = current;
                    pathQueue.Enqueue(neighbor);
                }
            }
        }

        if (!parentMap.ContainsKey(target)) return null; // Path blocked / not found. Re-routes shouldn't happen unless broken manually.

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
            
            Sequence stepSequence = DOTween.Sequence();
            
            float horizontalOffset = 2; 
            float startOffset = -(partyMemberIcons.Length - 1) * horizontalOffset * 0.5f; 

            for (int pIndex = 0; pIndex < spawnedPartyIcons.Count; pIndex++)
            {
                Vector3 destOffsetPosition = new Vector3(startOffset + horizontalOffset * pIndex, 0, iconZOffset);

                stepSequence.Join(spawnedPartyIcons[pIndex].transform.DOMove(targetPosition + destOffsetPosition, 0.4f)
                    .SetEase(Ease.InOutSine));
            }

            // Wait for characters animation layout gap to finish stepping.
            yield return stepSequence.WaitForCompletion();
            
            currentNodeId = nextNode;
            
            if (gameStatsManager != null)
            {
                gameStatsManager.SaveCurrentNodeId(currentNodeId);
            }
            
            // NOTE: Here we iterate steps and we currently can implement step tracking logic, later integrating the `CalendarController` hooks.
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
            GameObject newIcon = Instantiate(partyMemberIcons[j], partyMemberIconPosition + offsetPosition, Quaternion.identity);
            
            spawnedPartyIcons.Add(newIcon);
        }
    }

    private void UnlockLevel(GameObject mapNode)
    {
        mapNode.GetComponentInChildren<MapNodeController>().currentLockStatus = MapNodeController.LockStatus.levelUnlocked;
    }

    private void UpdateNodeVisuals(GameObject mapNode)
    {
        MapNodeController nodeController = mapNode.GetComponentInChildren<MapNodeController>();
        Color color = nodeController != null ? GetNodeTypeColor(nodeController.type) : Color.green;
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

    private NodeType GenerateNodeType()
    {
        float totalWeight = regularBattleWeight + puzzleBattleWeight + minibossBattleWeight + bossBattleWeight;
        float randomVal = Random.Range(0, totalWeight);

        if (randomVal < regularBattleWeight) return NodeType.RegularBattle;
        randomVal -= regularBattleWeight;

        if (randomVal < puzzleBattleWeight) return NodeType.PuzzleBattle;
        randomVal -= puzzleBattleWeight;

        if (randomVal < minibossBattleWeight) return NodeType.MinibossBattle;

        return NodeType.BossBattle;
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