using System.Collections.Generic;
using UnityEngine;

public class OverworldMapGenerator : MonoBehaviour
{
    public List<Domain> domains = new List<Domain>();
    public GameObject mapNode;
    public Transform mapNodeTransform;

    [Header("Configuration")]
    [Tooltip("Assegna qui la tua configurazione ScriptableObject")]
    public MapGenerationConfig config;

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

    public Transform currentMapNodeTransform;

    private LineRenderer lineRenderer;
    private List<Vector3> nodePositions = new List<Vector3>();
    private int currentDomainId = 0;

    // --- Variabili per il tracciamento e la distruzione ---
    private Domain currentDomain;
    private List<GameObject> spawnedNodes = new List<GameObject>();
    private List<GameObject> spawnedPartyIcons = new List<GameObject>();
    private bool needsRegeneration = false;

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

    // Aggiungiamo comunque OnValidate per sicurezza sulla checkbox
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

        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        // --- FIX LINE RENDERER TWISTING ---
        // Imposta l'allineamento della linea rispetto all'asse Z del Transform anziché alla telecamera
        lineRenderer.alignment = LineAlignment.TransformZ;
        
        // Ruota il Transform in modo che il suo asse Z guardi verso l'alto (sull'asse Y globale).
        // (Assicurati che questo script sia assegnato a un GameObject che può essere ruotato senza rompere altro, 
        // altrimenti è meglio creare un GameObject figlio dedicato solo per il LineRenderer).
        lineRenderer.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        // Aggiunge vertici per arrotondare gli angoli: evita del tutto gli "spigoli rotti"
        lineRenderer.numCornerVertices = 4;
        lineRenderer.numCapVertices = 4;
        // ----------------------------------

        // Assegniamo il materiale se è stato fornito
        if (pathLineMaterial != null)
        {
            lineRenderer.material = pathLineMaterial;
        }
        
        lineRenderer.positionCount = domainLevelSelection.levelList.Length;
        lineRenderer.startWidth = 0.5f;
        lineRenderer.endWidth = 0.5f;
        lineRenderer.useWorldSpace = true; // Assicura l'allineamento assoluto

        List<Vector3> scatteredPositions = new List<Vector3>();

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
        }

        scatteredPositions.Sort((a, b) => a.x.CompareTo(b.x));

        for (int i = 0; i < domainLevelSelection.levelList.Length; i++)
        {
            Vector3 finalPosition = scatteredPositions[i];
            
            GameObject newNode = Instantiate(mapNode, finalPosition, Quaternion.identity);
            
            spawnedNodes.Add(newNode);

            newNode.GetComponent<EnemySelection>().enemyParty = domainLevelSelection.levelList[i].enemyPartyData;
            newNode.GetComponent<EnemySelection>().levelNumber = domainLevelSelection.levelList[i].levelNumber;
            newNode.GetComponent<EnemySelection>().mapData = domainLevelSelection.levelList[i].map;

            if (i == highestUnlockedLevel)
            {
                currentMapNodeTransform = newNode.transform;
                UpdateNodeVisuals(newNode);
                UnlockLevel(newNode);
                UpdatePartyMemberVisuals(newNode);
            }
            else
            {
                newNode.GetComponentInChildren<MeshRenderer>().material.color = Color.gray;
                newNode.GetComponentInChildren<MapNodeController>().currentLockStatus = MapNodeController.LockStatus.levelLocked;
            }

            nodePositions.Add(finalPosition);

            // Alziamo la posizione della linea rispetto al nodo per evitare compenetrazioni (Z-Fighting)
            Vector3 linePosition = finalPosition + new Vector3(0, lineVerticalOffset, 0);
            lineRenderer.SetPosition(i, linePosition);
        }
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
        mapNode.GetComponentInChildren<MeshRenderer>().material.color = Color.green;
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

        nodePositions.Clear();
        if (lineRenderer != null) lineRenderer.positionCount = 0;
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