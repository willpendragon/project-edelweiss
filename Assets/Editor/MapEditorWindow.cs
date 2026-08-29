using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public partial class MapEditorWindow : EditorWindow
{
    // --- MAP PARAMETERS ---
    private int gridWidth = 10;
    private int gridHeight = 10; // Z
    private int gridDepth = 10;  // Y

    public float inBetweenTilesXOffset = 0f;
    public float inBetweenTilesZOffset = 0f;

    // --- PREFABS & POOLS ---
    private GameObject tilePrefab;
    private GameObject decorationPrefab;
    private GameObject unitPrefab;
    private GameObject deityShardPrefab; // NEW
    private GameObject interactablePrefab;
    private GameObject enemyPrefab;
    private GameObject environmentPrefab;

    public List<GameObject> decorationPrefabs = new List<GameObject>();
    public List<GameObject> unitPrefabs = new List<GameObject>();
    public List<GameObject> deityShardPrefabs = new List<GameObject>(); // NEW
    public List<GameObject> interactablePrefabs = new List<GameObject>();
    public List<GameObject> enemyPrefabs = new List<GameObject>();
    public List<GameObject> environmentPrefabs = new List<GameObject>();

    private SerializedObject _so;
    private SerializedProperty _decorationsProp;
    private SerializedProperty _unitsProp;
    private SerializedProperty _deityShardProp; // NEW
    private SerializedProperty _interactablesProp;
    private SerializedProperty _enemiesProp;
    private SerializedProperty _environmentsProp;

    private int _selectedDecorationIndex = 0;
    private int _selectedUnitIndex = 0;
    private int _selectedDeityShardIndex = 0; // NEW
    private int _selectedInteractableIndex = 0;
    private int _selectedEnemyIndex = 0;
    private int _selectedEnvironmentIndex = 0;

    // --- UI SCROLLS ---
    private Vector2 _decorScrollPos;
    private Vector2 _unitScrollPos;
    private Vector2 _deityShardScrollPos; // NEW
    private Vector2 _interactableScrollPos;
    private Vector2 _beaconScrollPos;
    private Vector2 _mainScrollPos; 
    private Vector2 _enemyScrollPos;
    private Vector2 _envScrollPos;

    // --- STATE ---
    private MapData currentMap;
    private TileType selectedTileType = TileType.Basic;

    private Dictionary<Vector3Int, GameObject> tiles = new Dictionary<Vector3Int, GameObject>();
    private Dictionary<Vector3Int, GameObject> decorations = new Dictionary<Vector3Int, GameObject>();
    private Dictionary<Vector3Int, GameObject> spawnedUnits = new Dictionary<Vector3Int, GameObject>();
    private Dictionary<Vector3Int, GameObject> spawnedDeityShards = new Dictionary<Vector3Int, GameObject>(); // Add this
    private Dictionary<Vector3Int, GameObject> spawnedInteractables = new Dictionary<Vector3Int, GameObject>();
    private Dictionary<Vector3Int, GameObject> spawnedEnemies = new Dictionary<Vector3Int, GameObject>();
    private List<GameObject> spawnedEnvironments = new List<GameObject>();
    private List<GameObject> spawnedLights = new List<GameObject>();

    private bool isPlacingTile = false;
    private bool isPlacingDecoration = false;
    private bool isPlacingUnit = false;
    private bool isPlacingDeityShard = false; // NEW
    private bool isPlacingInteractable = false;
    private bool isPlacingEnemy = false;
    private bool isDeletingTile = false;
    private bool isBucketMode = false;
    private bool clearOnClose = false;

    private Vector3Int lastGridPosition = new Vector3Int(-1, -1, -1);
    private Vector3Int lastPaintedPosition = new Vector3Int(-1, -1, -1);
    private int brushSize = 1;

    public float paintDelay = 0.15f;
    private double lastPaintTime = 0.0;
    private Texture2D headerImage;
    private int currentLinkID = 1;

    private Camera referenceCamera; // <--- NEW: Camera Reference
    private Light referenceDirectionalLight; // <--- NEW: Directional Light Reference
    private UnityEngine.Rendering.Volume referenceGlobalVolume; // <--- NEW: Global Volume Reference


    [MenuItem("Window/Map Editor")]
    public static void ShowWindow() => GetWindow<MapEditorWindow>("Map Editor");

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        headerImage = Resources.Load<Texture2D>("MapEditorBanner");

        tilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(EditorPrefs.GetString("MapEditor_TilePrefabPath", ""));
        inBetweenTilesXOffset = EditorPrefs.GetFloat("MapEditor_OffsetX", 0f);
        inBetweenTilesZOffset = EditorPrefs.GetFloat("MapEditor_OffsetZ", 0f);
        clearOnClose = EditorPrefs.GetBool("MapEditor_ClearOnClose", false);
        paintDelay = EditorPrefs.GetFloat("MapEditor_PaintDelay", 0.15f);

        _so = new SerializedObject(this);
        _decorationsProp = _so.FindProperty("decorationPrefabs");
        _unitsProp = _so.FindProperty("unitPrefabs");
        _deityShardProp = _so.FindProperty("deityShardPrefabs"); // NEW
        _interactablesProp = _so.FindProperty("interactablePrefabs");
        _enemiesProp = _so.FindProperty("enemyPrefabs");
        _environmentsProp = _so.FindProperty("environmentPrefabs");

        LoadDecoPrefabsFromPrefs();
        SyncDictionaryFromScene();
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        if (clearOnClose)
        {
            GenerateMap();
            Debug.Log("Map Editor: Scene automatically cleared on close.");
        }
    }

    private void OnGUI()
    {
        _mainScrollPos = EditorGUILayout.BeginScrollView(_mainScrollPos);

        if (headerImage != null)
        {
            float imageWidth = EditorGUIUtility.currentViewWidth;
            float imageHeight = imageWidth * ((float)headerImage.height / headerImage.width);
            GUILayout.Label(headerImage, GUILayout.Width(imageWidth), GUILayout.Height(Mathf.Clamp(imageHeight, 50, 150)));
        }

        GUILayout.Label("Map Dimensions", EditorStyles.boldLabel);
        gridWidth = EditorGUILayout.IntField("Grid Width (X)", gridWidth);
        gridDepth = EditorGUILayout.IntField("Grid Depth/Height (Y)", gridDepth);
        gridHeight = EditorGUILayout.IntField("Grid Length (Z)", gridHeight);

        EditorGUILayout.Space();
        GUILayout.Label("Map Offsets & Experience", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        inBetweenTilesXOffset = EditorGUILayout.FloatField("X Offset", inBetweenTilesXOffset);
        inBetweenTilesZOffset = EditorGUILayout.FloatField("Z Offset (Old Y)", inBetweenTilesZOffset);
        paintDelay = EditorGUILayout.Slider("Paint Cooldown (sec)", paintDelay, 0.05f, 1.0f);
        if (EditorGUI.EndChangeCheck())
        {
            EditorPrefs.SetFloat("MapEditor_OffsetX", inBetweenTilesXOffset);
            EditorPrefs.SetFloat("MapEditor_OffsetZ", inBetweenTilesZOffset);
            EditorPrefs.SetFloat("MapEditor_PaintDelay", paintDelay);
            SceneView.RepaintAll();
        }

        EditorGUILayout.Space();
        GUILayout.Label("Assets & Properties", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        tilePrefab = (GameObject)EditorGUILayout.ObjectField("Tile Prefab", tilePrefab, typeof(GameObject), false);
        if (EditorGUI.EndChangeCheck() && tilePrefab != null)
        {
            EditorPrefs.SetString("MapEditor_TilePrefabPath", AssetDatabase.GetAssetPath(tilePrefab));
        }

        DrawPrefabPool("Decoration Pool", ref _decorationsProp, decorationPrefabs, ref _selectedDecorationIndex, ref decorationPrefab, ref _decorScrollPos, SaveDecoPrefabsToPrefs);
        DrawPrefabPool("Player Units Pool", ref _unitsProp, unitPrefabs, ref _selectedUnitIndex, ref unitPrefab, ref _unitScrollPos, SaveUnitPrefabsToPrefs);
        DrawPrefabPool("Deity Shards Pool", ref _deityShardProp, deityShardPrefabs, ref _selectedDeityShardIndex, ref deityShardPrefab, ref _deityShardScrollPos, SaveDeityShardPrefabsToPrefs); // NEW
        DrawPrefabPool("Interactables Pool", ref _interactablesProp, interactablePrefabs, ref _selectedInteractableIndex, ref interactablePrefab, ref _interactableScrollPos, SaveInteractablePrefabsToPrefs);
        DrawPrefabPool("Enemy Units Pool", ref _enemiesProp, enemyPrefabs, ref _selectedEnemyIndex, ref enemyPrefab, ref _enemyScrollPos, SaveEnemyPrefabsToPrefs);
        DrawPrefabPool("Environment Props Pool", ref _environmentsProp, environmentPrefabs, ref _selectedEnvironmentIndex, ref environmentPrefab, ref _envScrollPos, SaveEnvironmentPrefabsToPrefs);

        EditorGUILayout.Space();

        // --- UPDATED: Detect assignment in slot and update Grid Dimensions automatically ---
        EditorGUI.BeginChangeCheck();
        currentMap = (MapData)EditorGUILayout.ObjectField("Current Map Asset", currentMap, typeof(MapData), false);
        if (EditorGUI.EndChangeCheck() && currentMap != null)
        {
            gridWidth = currentMap.horizontalSize;
            gridDepth = currentMap.depthSize;
            gridHeight = currentMap.verticalSize;
            GUI.FocusControl(null); // Deselect to allow quick updates
        }
        // ----------------------------------------------------------------------------------
        
        EditorGUILayout.Space();
        
        // 1. Manually identify which TileTypes you still want to paint as base floor architecture
        TileType[] allowedTileTypes = new TileType[] 
        {
            TileType.Basic,
            TileType.Chest,
            TileType.MinibossChest,
            TileType.BossChest,
            TileType.DeityTile, // <--- Added DeityTile
            TileType.IngredientChest
        };

        // 2. Convert them to string arrays for the Editor UI
        string[] displayOptions = allowedTileTypes.Select(t => t.ToString()).ToArray();

        // 3. Find the current index so the dropdown stays on what you selected
        int currentIndex = System.Array.IndexOf(allowedTileTypes, selectedTileType);
        if (currentIndex < 0) currentIndex = 0; // Default to 'Basic' if currently selected an invalid type

        // 4. Draw the customized popup menu with a tooltip
        int newIndex = EditorGUILayout.Popup(new GUIContent("Tile Type", "Paint structure tiles or specialized spawn tiles. Hover for docs."), currentIndex, displayOptions);
        selectedTileType = allowedTileTypes[newIndex];
        
        // --- NEW: Documentation Notification ---
        if (selectedTileType == TileType.MinibossChest || selectedTileType == TileType.BossChest)
        {
            string reqName = selectedTileType == TileType.MinibossChest ? "MiniBossChest" : "BossChest";
            EditorGUILayout.HelpBox($"The '{selectedTileType}' strictly requires a prefab named '{reqName}' inside any 'Resources' folder to load properly at runtime.", MessageType.Info);
        }
        else if (selectedTileType == TileType.IngredientChest)
        {
            EditorGUILayout.HelpBox("The 'IngredientChest' prefab needs a baked-in ChestUnit (Unit Template = your IngredientChestTemplate asset, with Prize Override = your IngredientPrize asset) to avoid any Resources-folder naming requirement. Without a baked-in ChestUnit, it falls back to loading 'IngredientChest'/'IngredientChestTemplate'/'IngredientPrize' by name from a 'Resources' folder.", MessageType.Info);
        }

        EditorGUILayout.Space();
        clearOnClose = EditorGUILayout.Toggle("Clear Tiles when Closing Editor", clearOnClose);
        if (GUI.changed) EditorPrefs.SetBool("MapEditor_ClearOnClose", clearOnClose);

        // --- NEW: Camera Settings Section ---
        EditorGUILayout.Space();
        GUILayout.Label("Map Camera Settings", EditorStyles.boldLabel);
        referenceCamera = (Camera)EditorGUILayout.ObjectField("Scene Camera Reference", referenceCamera, typeof(Camera), true);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Save Camera to Map Asset"))
        {
            if (currentMap == null)
            {
                Debug.LogWarning("Map Editor: Assign a Current Map Asset first!");
            }
            else if (referenceCamera == null)
            {
                Debug.LogWarning("Map Editor: Assign a Scene Camera Reference to save its transform!");
            }
            else
            {
                Undo.RecordObject(currentMap, "Save Camera Settings");
                currentMap.overrideCameraSettings = true;
                currentMap.cameraPosition = referenceCamera.transform.position;
                currentMap.cameraRotation = referenceCamera.transform.eulerAngles;
                currentMap.cameraZoom = referenceCamera.fieldOfView;
                currentMap.isOrthographic = referenceCamera.orthographic;
                currentMap.orthographicSize = referenceCamera.orthographicSize;
                
                EditorUtility.SetDirty(currentMap);
                AssetDatabase.SaveAssets();
                Debug.Log("Map Editor: True Camera settings successfully saved to MapData!");
            }
        }

        if (GUILayout.Button("Apply Map Camera to Scene"))
        {
            SyncCameraFromMap();
        }
        EditorGUILayout.EndHorizontal();
        // ------------------------------------

        EditorGUILayout.Space();
        // --- NEW: Directional Light Settings Section ---
        EditorGUILayout.Space();
        GUILayout.Label("Map Directional Light Settings", EditorStyles.boldLabel);
        referenceDirectionalLight = (Light)EditorGUILayout.ObjectField("Scene Dir Light Ref", referenceDirectionalLight, typeof(Light), true);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Save Dir Light to Asset"))
        {
            if (currentMap == null)
            {
                Debug.LogWarning("Map Editor: Assign a Current Map Asset first!");
            }
            else if (referenceDirectionalLight == null)
            {
                Debug.LogWarning("Map Editor: Assign a Scene Directional Light Reference to save it!");
            }
            else
            {
                Undo.RecordObject(currentMap, "Save Directional Light Settings");
                currentMap.overrideDirectionalLight = true;
                currentMap.directionalLightRotation = referenceDirectionalLight.transform.eulerAngles;
                currentMap.directionalLightColor = referenceDirectionalLight.color;
                currentMap.directionalLightIntensity = referenceDirectionalLight.intensity;
                
                EditorUtility.SetDirty(currentMap);
                AssetDatabase.SaveAssets();
                Debug.Log("Map Editor: Directional Light settings successfully saved to MapData!");
            }
        }

        if (GUILayout.Button("Apply Dir Light to Scene"))
        {
            SyncDirectionalLightFromMap();
        }
        EditorGUILayout.EndHorizontal();
        // ------------------------------------

        // --- NEW: Global Volume Settings Section ---
        EditorGUILayout.Space();
        GUILayout.Label("Map Global Volume Settings", EditorStyles.boldLabel);
        referenceGlobalVolume = (UnityEngine.Rendering.Volume)EditorGUILayout.ObjectField("Scene Global Volume Ref", referenceGlobalVolume, typeof(UnityEngine.Rendering.Volume), true);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Save Global Volume to Asset"))
        {
            if (currentMap == null)
            {
                Debug.LogWarning("Map Editor: Assign a Current Map Asset first!");
            }
            else
            {
                UnityEngine.Rendering.VolumeProfile profileToSave = null;

                if (referenceGlobalVolume != null)
                {
                    profileToSave = referenceGlobalVolume.sharedProfile;
                }
                else
                {
                    Debug.LogWarning("Map Editor: No Scene Global Volume reference provided. If you want to assign a profile directly from the project folder, you can do it on the MapData asset directly.");
                }

                if (profileToSave != null || currentMap.globalVolumeProfile != null)
                {
                    Undo.RecordObject(currentMap, "Save Global Volume Settings");
                    currentMap.overrideGlobalVolume = true;
                    
                    if (profileToSave != null)
                        currentMap.globalVolumeProfile = profileToSave;

                    EditorUtility.SetDirty(currentMap);
                    AssetDatabase.SaveAssets();
                    Debug.Log("Map Editor: Global Volume profile successfully saved to MapData!");
                }
            }
        }

        if (GUILayout.Button("Apply Volume to Scene"))
        {
            SyncGlobalVolumeFromMap();
        }
        EditorGUILayout.EndHorizontal();
        // ------------------------------------

        EditorGUILayout.Space();
        if (GUILayout.Button("Generate/Clear Map")) GenerateMap();

        EditorGUILayout.Space();
        
        // Update Select check to include deity shard mode
        // Add !isPlacingDeityShard to the negation list
        bool isSelectMode = !isPlacingTile && !isPlacingInteractable && !isPlacingDecoration && !isPlacingUnit && !isPlacingEnemy && !isPlacingDeityShard && !isDeletingTile;
        
        // --- BUTTON ROW 1 ---
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Toggle(isSelectMode, "Select / View", "Button")) { SetMode(); } 
        if (GUILayout.Toggle(isPlacingTile, "Paint Tile", "Button")) { SetMode(tile: true); }
        if (GUILayout.Toggle(isPlacingInteractable, "Paint Interactable", "Button")) { SetMode(interactable: true); }
        if (GUILayout.Toggle(isPlacingDecoration, "Paint Decoration", "Button")) { SetMode(deco: true); }
        EditorGUILayout.EndHorizontal();

        // --- BUTTON ROW 2 ---
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Toggle(isPlacingUnit, "Paint Player", "Button")) { SetMode(unit: true); }
        if (GUILayout.Toggle(isPlacingDeityShard, "Paint DeityShard", "Button")) { SetMode(deityShard: true); } // NEW
        if (GUILayout.Toggle(isPlacingEnemy, "Paint Enemy", "Button")) { SetMode(enemy: true); }
        if (GUILayout.Toggle(isDeletingTile, "Delete Mode", "Button")) { SetMode(delete: true); }
        isBucketMode = GUILayout.Toggle(isBucketMode, "Bucket Fill (B)", "Button");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        brushSize = EditorGUILayout.IntSlider("Brush Size (Number Keys 1-6)", brushSize, 1, 6);

        EditorGUILayout.Space();
        if (GUILayout.Button("Save Map to Asset", GUILayout.Height(30))) SaveMap();
        if (GUILayout.Button("Load Map from Asset", GUILayout.Height(30))) LoadFromAsset();

        EditorGUILayout.Space();
        if (isPlacingInteractable)
        {
            GUI.color = Color.yellow;
            currentLinkID = EditorGUILayout.IntField("Interactable Link ID", currentLinkID);
            GUILayout.Label("Interactables painted now will share this ID.", EditorStyles.helpBox);
            GUI.color = Color.white;
        }

        EditorGUILayout.Space();
        GUILayout.Label("Freeform Environment Setup", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Spawn Prop at Camera Target", GUILayout.Height(30)))
        {
            SpawnEnvironmentProp();
        }
        
        // --- NEW: Dedicated Save Button ---
        if (GUILayout.Button("Save Environments to Map Asset", GUILayout.Height(30)))
        {
            SaveEnvironmentsToMap();
        }
        EditorGUILayout.EndHorizontal();

        // --- NEW: Spawn Light Buttons ---
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Spawn Point Light at Camera Target", GUILayout.Height(30)))
        {
            SpawnEditorLight();
        }

        if (GUILayout.Button("Save Lights to Map Asset", GUILayout.Height(30)))
        {
            SaveLightsToMap();
        }
        EditorGUILayout.EndHorizontal();
        // --------------------------------------------------------------------

        EditorGUILayout.EndScrollView();
    }

    // The bucket toggle is passed so it correctly enables/disables without affecting other tools
    public void SetMode(bool tile = false, bool deco = false, bool unit = false, bool deityShard = false, bool enemy = false, bool interactable = false, bool delete = false, bool bucket = false)
    {
        isPlacingTile = tile;
        isPlacingDecoration = deco;
        isPlacingUnit = unit;
        isPlacingDeityShard = deityShard; // NEW
        isPlacingEnemy = enemy;
        isPlacingInteractable = interactable;
        isDeletingTile = delete;

        if (bucket) isBucketMode = true;
        else if (!tile && !deco && !unit && !enemy && !interactable && !delete) isBucketMode = false;
    }

    private void DrawPrefabPool(string label, ref SerializedProperty prop, List<GameObject> prefabs, ref int index, ref GameObject activePrefab, ref Vector2 scroll, System.Action saveCallback)
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

        _so.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(prop, new GUIContent(label), true);
        if (EditorGUI.EndChangeCheck())
        {
            _so.ApplyModifiedProperties();
            saveCallback?.Invoke();
            if (index >= prefabs.Count) index = 0;
            activePrefab = prefabs.Count > 0 ? prefabs[index] : null;
        }

        if (prefabs.Count > 0)
        {
            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(105));
            EditorGUILayout.BeginHorizontal();

            for (int i = 0; i < prefabs.Count; i++)
            {
                if (prefabs[i] == null) continue;
                Texture2D preview = AssetPreview.GetAssetPreview(prefabs[i]);
                GUI.backgroundColor = (i == index) ? Color.cyan : Color.white;

                if (GUILayout.Button(preview != null ? new GUIContent(preview, prefabs[i].name) : new GUIContent(prefabs[i].name), GUILayout.Width(64), GUILayout.Height(64)))
                {
                    index = i;
                    activePrefab = prefabs[i];
                }
                GUI.backgroundColor = Color.white;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();
        }
    }

    private void SyncCameraFromMap()
    {
        if (currentMap == null)
        {
            Debug.LogWarning("Map Editor: Assign a Current Map Asset first to sync the camera!");
            return;
        }

        if (!currentMap.overrideCameraSettings)
        {
            Debug.Log("Map Editor: Current Map Asset does not have camera override active. Skipping camera sync.");
            return;
        }

        if (referenceCamera == null)
        {
            // Fallback to Main Camera if reference is null
            referenceCamera = Camera.main;
            if (referenceCamera == null)
            {
                Debug.LogWarning("Map Editor: No Scene Camera Reference assigned and no Main Camera found in the scene! Cannot apply map camera settings.");
                return;
            }
            else
            {
                Debug.Log("Map Editor: Auto-assigned Main Camera as Scene Camera Reference.");
            }
        }

        Undo.RecordObject(referenceCamera.transform, "Sync Camera Transform");
        Undo.RecordObject(referenceCamera, "Sync Camera Properties");

        referenceCamera.transform.position = currentMap.cameraPosition;
        referenceCamera.transform.eulerAngles = currentMap.cameraRotation;
        referenceCamera.orthographic = currentMap.isOrthographic;
        
        if (currentMap.isOrthographic)
            referenceCamera.orthographicSize = currentMap.orthographicSize;
        else
            referenceCamera.fieldOfView = currentMap.cameraZoom;

        Debug.Log($"Map Editor: Successfully applied camera settings from '{currentMap.name}' to '{referenceCamera.name}'.");
    }

    // --- NEW: Point Light Spawning Logic ---
    private void SpawnEditorLight()
    {
        Vector3 spawnPosition = referenceCamera != null ? referenceCamera.transform.position + referenceCamera.transform.forward * 5f : Vector3.zero;
        
        GameObject lightObj = new GameObject($"MapLight_{System.Guid.NewGuid().ToString().Substring(0,5)}");
        Light lightComp = lightObj.AddComponent<Light>();
        lightComp.type = LightType.Point;
        lightComp.range = 10f;
        
        lightObj.transform.position = spawnPosition;
        
        Undo.RegisterCreatedObjectUndo(lightObj, "Spawn Light");
        spawnedLights.Add(lightObj);
        Selection.activeGameObject = lightObj;
    }
    // -------------------------------------------------------------------------

    // --- NEW: Directional Light Sync Logic ---
    private void SyncDirectionalLightFromMap()
    {
        if (currentMap == null) return;
        if (!currentMap.overrideDirectionalLight) return;

        if (referenceDirectionalLight == null)
        {
            // Auto-detect a directional light if the slot is empty
            referenceDirectionalLight = GameObject.FindObjectsOfType<Light>().FirstOrDefault(l => l.type == LightType.Directional);
            if (referenceDirectionalLight == null) return;
        }

        Undo.RecordObject(referenceDirectionalLight.transform, "Sync Light Transform");
        Undo.RecordObject(referenceDirectionalLight, "Sync Light Properties");

        referenceDirectionalLight.transform.eulerAngles = currentMap.directionalLightRotation;
        referenceDirectionalLight.color = currentMap.directionalLightColor;
        referenceDirectionalLight.intensity = currentMap.directionalLightIntensity;

        Debug.Log($"Map Editor: Successfully applied directional light settings from '{currentMap.name}'");
    }
    // ----------------------------------------------------------------------------------

    // --- NEW: Global Volume Sync Logic ---
    private void SyncGlobalVolumeFromMap()
    {
        if (currentMap == null) return;
        if (!currentMap.overrideGlobalVolume || currentMap.globalVolumeProfile == null) return;

        if (referenceGlobalVolume == null)
        {
            referenceGlobalVolume = GameObject.FindObjectsOfType<UnityEngine.Rendering.Volume>().FirstOrDefault(v => v.isGlobal);
            if (referenceGlobalVolume == null) return;
        }

        Undo.RecordObject(referenceGlobalVolume, "Sync Global Volume");
        referenceGlobalVolume.sharedProfile = currentMap.globalVolumeProfile;

        Debug.Log($"Map Editor: Successfully applied global volume profile from '{currentMap.name}'");
    }
    // ----------------------------------------------------------------------------------
}