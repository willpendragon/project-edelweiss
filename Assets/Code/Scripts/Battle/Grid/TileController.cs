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
    Triad
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
    public int tileXCoordinate;
    public int tileYCoordinate;
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
        if (detectedUnit == null)
            return;
        if (detectedUnit.CompareTag("Player"))
        {
            var unitSelection = FindAnyObjectByType<UnitSelectionController>();
            unitSelection.SelectPlayerUnit(detectedUnit.GetComponent<Unit>());
        }
        else if (detectedUnit.CompareTag("Enemy"))
        {

        }
        OnClickedOnTile(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (detectedUnit != null && detectedUnit.CompareTag("ActivePlayerUnit"))
            return;

        // Show and position the cursor over the tile at Y = 0.57.
        if (cursorInstance != null)
        {
            cursorInstance.transform.position = new Vector3(transform.position.x, 0.57f, transform.position.z);
            cursorInstance.SetActive(true);
        }
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