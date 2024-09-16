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


public class TileController : MonoBehaviour, IPointerClickHandler
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
    public TileType tileType;  // New property for tile type

    [Header("Visuals")]

    public GameObject targetIcon;
    public TileShaderController tileShaderController;
    public GameObject tilePrefabSprite;

    // A* Pathfinding properties
    public int gCost;
    public int hCost;
    public int FCost { get { return gCost + hCost; } }
    public TileController parent;

    public float clickCooldown = 0.5f; // Cooldown in seconds between clicks
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
            //If Waiting for Confirmation is True
            currentPlayerAction.Execute();
        }
    }
    public void HandleTileDeselection()
    {
        currentPlayerAction.Deselect();
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
            //Need to Use Switch Case (good enough for now)
            //Update Selected Character Values on UI
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