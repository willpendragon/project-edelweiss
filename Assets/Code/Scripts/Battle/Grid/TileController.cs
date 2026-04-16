using Edelweiss.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public enum SingleTileStatus
{
    basic,
    selectionMode,
    waitingForConfirmationMode,
    characterSelectionModeActive,
    selectedPlayerUnitOccupiedTile,
}

public enum SingleTileCondition
{
    free,
    occupied,
    occupiedByDeity
}

public enum TileCurseStatus
{
    notCursed,
    cursed
}

public enum TileType
{
    Basic,
    ActivationPlatform,
    Obstacle,
    Mirror,
    Triad,
    Chest,
    MinibossChest,
    BossChest,
    Environment, // <-- NEW: Used for solid decorative tiles
    Beacon
}

public enum TileElement // Such properties could be moved in an SO.
{
    Water,
    Ice,
    Lighting,
    Fire
}

public class TileController : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{

    [SerializeField] private Transform tileEffects;

    private void Reset()
    {
        // Auto-assign for convenience
        if (tileEffects == null)
            tileEffects = transform.Find("TileEffects");
    }

    private void OnEnable()
    {
        if (tileEffects == null)
            return;

        // Runtime ALWAYS shows effects
        if (Application.isPlaying)
        {
            tileEffects.gameObject.SetActive(true);
            return;
        }

#if UNITY_EDITOR
        // Editor ONLY: hide effects visually in Scene View
        SceneVisibilityManager.instance.Hide(tileEffects.gameObject, true);
#endif
    }

    [Header("Gameplay Logic")]
    public GameObject detectedUnit;
    
    // Sostituiamo le coordinate X e Y singole con un Vector3Int completo
    public Vector3Int gridPosition; 
    
    // Mantenute per compatibilità immediata se altri script vi accedono, 
    // ma ti consiglio di migrare tutto a 'gridPosition.x', 'gridPosition.y', 'gridPosition.z' in futuro.
    public int tileXCoordinate { get => gridPosition.x; set => gridPosition.x = value; }
    public int tileYCoordinate { get => gridPosition.z; set => gridPosition.z = value; } // L'estetica dice "Y" per i vecchi script, la logica usa "Z"
    public int tileElevation { get => gridPosition.y; set => gridPosition.y = value; } // Nuova Y / Altezza
    

    public IPlayerAction<TileController> currentPlayerAction = new SelectUnitPlayerAction();
    public MeleePlayerAction meleeAction;
    public GameObject tileCurrentFieldPrize;
    [SerializeField] PrizeCollectionHelper _prizeCollectionHelper;

    [Header("State Machines")]
    public SingleTileStatus currentSingleTileStatus;
    public SingleTileCondition currentSingleTileCondition;
    public TileCurseStatus currentTileCurseStatus;

    [Header("Tile Type")]
    public TileType tileType;
    public TileElement tileElement;

    [Header("Visuals")]
    public GameObject targetIcon;
    public TileShaderController tileShaderController;
    public GameObject tilePrefabSprite;

    [Header("Cursor Visual")]
    public GameObject cursorPrefab; // Reference to the cursor prefab
    private GameObject cursorInstance; // Instance of the cursor prefab
    [SerializeField] string _actionButtonTag = "ActionButton";

    [Header("A* Pathfinding properties")]
    public int gCost;
    public int hCost;
    public int FCost { get { return gCost + hCost; } }

    public TileController parent;
    public PrizeCollectionHelper PrizeCollectionHelper => _prizeCollectionHelper;

    public delegate void UpdateEnemyTargetUnitProfile(GameObject detectedUnit);
    public static event UpdateEnemyTargetUnitProfile OnUpdateEnemyTargetUnitProfile;

    public delegate void ClickedOnTile(TileController tileController);
    public static event ClickedOnTile OnClickedOnTile;

    public delegate void PointerAwayFromTile();
    public static event PointerAwayFromTile OnPointerAwayFromTile;

    void Start()
    {
        currentTileCurseStatus = TileCurseStatus.notCursed;

        string currentSceneName = SceneManager.GetActiveScene().name;
        if (currentSceneName == "overworld_map")
        {
            tilePrefabSprite.GetComponent<SpriteRenderer>().enabled = false;
        }

        // Instantiate the cursor prefab but keep it inactive initially.
        if (cursorPrefab != null)
        {
            cursorInstance = Instantiate(cursorPrefab);
            cursorInstance.SetActive(false);
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (GridManager.IsUnitMoving)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, 100f);

        // Sort the hits by distance so we evaluate them front-to-back
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        TileController actualTargetTile = this;
        int highestPriority = -1;

        foreach (var hit in hits)
        {
            TileController hitTile = hit.collider.GetComponentInParent<TileController>();
            if (hitTile != null)
            {
                int currentPriority = 0; // default for an empty tile

                if (hitTile.detectedUnit != null)
                {
                    // Highest priority: Unselected playable units
                    if (hitTile.detectedUnit.CompareTag("Player"))
                        currentPriority = 2;
                    // Lower priority: Enemies, Chests, or the already Active Unit
                    else if (hitTile.detectedUnit.CompareTag("Enemy") || hitTile.detectedUnit.CompareTag("Chest") || hitTile.detectedUnit.CompareTag("ActivePlayerUnit"))
                        currentPriority = 1;
                }

                // Update best target if this tile is higher priority
                if (currentPriority > highestPriority)
                {
                    highestPriority = currentPriority;
                    actualTargetTile = hitTile;

                    // If we found the absolute highest priority (Unselected Player), stop looking deeper
                    if (highestPriority == 2)
                        break; 
                }
                else if (highestPriority == -1 && currentPriority == 0)
                {
                    // Fallback to the first empty tile if no units have been hit yet
                    highestPriority = 0;
                    actualTargetTile = hitTile;
                }
            }
        }

        if (actualTargetTile != null && actualTargetTile.detectedUnit != null)
        {
            var unitSelection = FindAnyObjectByType<UnitSelectionController>();
            
            if (actualTargetTile.detectedUnit.CompareTag("Player") || actualTargetTile.detectedUnit.CompareTag("ActivePlayerUnit"))
            {
                unitSelection.SelectPlayerUnit(actualTargetTile.detectedUnit.GetComponent<Unit>());
            }
            else if (actualTargetTile.detectedUnit.CompareTag("Enemy"))
            {
                unitSelection.SelectEnemy(actualTargetTile.detectedUnit.GetComponent<Unit>());
            }
        }

        OnClickedOnTile(actualTargetTile);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (detectedUnit != null && detectedUnit.CompareTag("ActivePlayerUnit"))
            return;

        // Show and position the cursor over the tile dynamically.
        if (cursorInstance != null)
        {
            float cursorY = transform.position.y + 0.57f; // Fallback temporaneo
            Collider tileCollider = GetComponentInChildren<Collider>();
            
            if (tileCollider != null)
            {
                // Posiziona il cursore esattamente sul margine superiore del collider (+0.07f per l'offset visivo per non compenetrare)
                cursorY = tileCollider.bounds.max.y + 0.07f;
            }

            cursorInstance.transform.position = new Vector3(transform.position.x, cursorY, transform.position.z);
            cursorInstance.SetActive(true);
        }
        BattleSFXManager.PlaySound(SoundType.UIHOVER);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        // Hide the cursor when exiting the Tile.
        if (cursorInstance != null)
        {
            cursorInstance.SetActive(false);
        }
    }
}