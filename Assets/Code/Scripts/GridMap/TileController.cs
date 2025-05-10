using DG.Tweening;
using System.Collections;
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

    private bool tileClickingAllowed;

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

        // Instantiate the cursor prefab but keep it inactive initially.
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
    private void TileControllerCooldown()
    {
        foreach (TileController tile in GridManager.Instance.gridTileControllers)
        {
            tileClickingAllowed = false;
            tile.gameObject.GetComponent<BoxCollider>().enabled = false;
            StartCoroutine(TileClickingCooldown(0.3f));
        }
    }

    IEnumerator TileClickingCooldown(float tileClickingCooldown)
    {
        yield return new WaitForSecondsRealtime(tileClickingCooldown);
        tileClickingAllowed = true;
        foreach (TileController tile in GridManager.Instance.gridTileControllers)
        {
            tileClickingAllowed = true;
            tile.gameObject.GetComponent<BoxCollider>().enabled = true;
        }
    }

    public void HandleTileSelection()
    {
        TileControllerCooldown();
        switch (currentSingleTileStatus)
        {
            case SingleTileStatus.selectionMode:
                HandleSelectionMode();
                break;

            case SingleTileStatus.waitingForConfirmationMode:
                HandleConfirmationMode();
                break;

            default:
                Debug.Log("Unhandled tile status");
                break;
        }
    }
    private void HandleSelectionMode()
    {
        if (currentPlayerAction is SelectUnitPlayerAction)
        {
            ExecutePlayerAction();
        }
        else
        {
            currentPlayerAction.Select(this);
        }
    }
    private void HandleConfirmationMode()
    {
        ExecutePlayerAction();
    }
    private void ExecutePlayerAction()
    {
        currentPlayerAction.Select(this);
        currentPlayerAction.Execute();
    }
    public void HandleTileDeselection()
    {
        currentPlayerAction.Deselect();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
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
            else if (fieldPrizeController != null && fieldPrizeController.fieldPrize.itemFieldPrizeType == ItemFieldPrizeType.PuzzleLevelKey)
            {
                GameStatsManager gameStatsManager = GameObject.FindGameObjectWithTag("GameStatsManager").GetComponent<GameStatsManager>();
                gameStatsManager.unlockedPuzzleKeys += 1;
                gameStatsManager.SaveUnlockedKeys(gameStatsManager.unlockedPuzzleKeys);
                Debug.Log("Added Key to Game Stats Manager and saved to game state");
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