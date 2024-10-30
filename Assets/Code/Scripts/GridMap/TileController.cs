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
    Mirror
}

public class TileController : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Gameplay Logic")]
    public GameObject detectedUnit;
    public int tileXCoordinate;
    public int tileYCoordinate;

    public IPlayerAction currentPlayerAction = new SelectUnitPlayerAction();
    public MeleePlayerAction meleeAction;
    public GameObject tileCurrentFieldPrize;

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

    // A* Pathfinding properties
    public int gCost;
    public int hCost;
    public int FCost { get { return gCost + hCost; } }
    public TileController parent;

    public float clickCooldown = 0.5f;
    private float lastClickTime;

    public delegate void UpdateEnemyTargetUnitProfile(GameObject detectedUnit);
    public static event UpdateEnemyTargetUnitProfile OnUpdateEnemyTargetUnitProfile;

    void Start()
    {
        currentTileCurseStatus = TileCurseStatus.notCursed;

        string currentSceneName = SceneManager.GetActiveScene().name;
        if (currentSceneName == "overworld_map")
        {
            tilePrefabSprite.GetComponent<SpriteRenderer>().enabled = false;
        }

        // Instantiate the cursor prefab but keep it inactive initially
        if (cursorPrefab != null)
        {
            cursorInstance = Instantiate(cursorPrefab);
            cursorInstance.SetActive(false);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && Time.time - lastClickTime > clickCooldown)
        {
            lastClickTime = Time.time;
            HandleTileSelection();
        }
        else if (eventData.button == PointerEventData.InputButton.Right && Time.time - lastClickTime > clickCooldown)
        {
            lastClickTime = Time.time;
            HandleTileDeselection();
        }
    }

    public void HandleTileSelection()
    {
        if (currentSingleTileStatus == SingleTileStatus.selectionMode)
        {
            Debug.Log("Selecting Tiles");
            currentPlayerAction.Select(this);
        }
        else if (currentSingleTileStatus == SingleTileStatus.waitingForConfirmationMode)
        {
            currentPlayerAction.Execute();
        }
    }

    public void HandleTileDeselection()
    {
        currentPlayerAction.Deselect();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Mouse entered tile: " + gameObject.name);

        // Show and position the cursor over the tile at Y = 0.57
        if (cursorInstance != null)
        {
            cursorInstance.transform.position = new Vector3(transform.position.x, 0.57f, transform.position.z);
            cursorInstance.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Mouse exited tile: " + gameObject.name);

        // Hide the cursor when exiting the tile
        if (cursorInstance != null)
        {
            cursorInstance.SetActive(false);
        }
    }

    public void CheckFieldPrizes(TileController destinationTile, Unit activePlayerUnit)
    {
        if (destinationTile != null && destinationTile.tileCurrentFieldPrize != null)
        {
            FieldPrizeController fieldPrizeController = destinationTile.tileCurrentFieldPrize.GetComponent<FieldPrizeController>();
            if (fieldPrizeController != null && fieldPrizeController.fieldPrize.itemFieldPrizeType == ItemFieldPrizeType.attackPowerUp)
            {
                activePlayerUnit.unitAttackPower += fieldPrizeController.fieldPrize.powerUpAmount;
            }
            else if (fieldPrizeController != null && fieldPrizeController.fieldPrize.itemFieldPrizeType == ItemFieldPrizeType.magicPowerUp)
            {
                activePlayerUnit.unitMagicPower += fieldPrizeController.fieldPrize.powerUpAmount;
            }
            UpdateCombatValues();
            Destroy(fieldPrizeController.gameObject);
            Debug.Log("Applied Power Up");
        }
    }

    private void UpdateCombatValues()
    {
        Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit")?.GetComponent<Unit>();
        if (activePlayerUnit != null)
        {
            activePlayerUnit.unitProfilePanel.GetComponent<PlayerProfileController>().activeCharacterAttackPower.text = activePlayerUnit.unitAttackPower.ToString();
            activePlayerUnit.unitProfilePanel.GetComponent<PlayerProfileController>().activeCharacterMagicPower.text = activePlayerUnit.unitMagicPower.ToString();
        }
    }
}
