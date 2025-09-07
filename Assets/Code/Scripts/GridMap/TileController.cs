using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static EnemyInfoPanelController;


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
public class TileController : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IEndDragHandler
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
    GameObject _enemyUnitPanel;

    [SerializeField] string _actionButtonTag = "ActionButton";

    // A* Pathfinding properties
    public int gCost;
    public int hCost;
    public int FCost { get { return gCost + hCost; } }
    public TileController parent;

    public delegate void UpdateEnemyTargetUnitProfile(GameObject detectedUnit);
    public static event UpdateEnemyTargetUnitProfile OnUpdateEnemyTargetUnitProfile;

    public delegate void ClickedOnTile(TileController tileController);
    public static event ClickedOnTile OnClickedOnTile;

    public delegate void PointerAwayFromTile();
    public static event PointerAwayFromTile OnPointerAwayFromTile;

    //public void OnPointerOver(PointerEventData eventData)
    //{
    //    bool foundAction = false;
    //    List<RaycastResult> results = new List<RaycastResult>();
    //    EventSystem.current.RaycastAll(eventData, results);

    //    foreach (var result in results)
    //    {
    //        if (result.gameObject.CompareTag("ActionButton"))
    //        {
    //            foundAction = true;
    //            result.gameObject.GetComponent<RadialMenuEntry>().FireAction();
    //            Debug.Log("Found Action Button");
    //            //OnEndDragCursorAcrossTile();
    //            ApplyProfileChangesWrapper();
    //            break;
    //        }
    //        else
    //        {
    //            Destroy(_enemyUnitPanel);
    //        }
    //    }

    //    if (!foundAction)
    //    {
    //        OnEndDragCursorAcrossTile();
    //        Debug.Log("Found no Action, closing menu");
    //    }
    //}
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
        if (detectedUnit == null)
            return;
        if (detectedUnit.CompareTag("Player"))
        {
            var unitSelection = FindAnyObjectByType<UnitSelectionController>();
            unitSelection.SelectPlayerUnit(detectedUnit.GetComponent<Unit>());
        }
        else if (detectedUnit.CompareTag("Enemy"))
        {
            var battleUI = GameObject.FindGameObjectWithTag("BattleInterfaceCanvas").GetComponent<BattleInterface>();
            _enemyUnitPanel = Instantiate(Resources.Load("CurrentlySelectedUnit") as GameObject, battleUI.battleDetails.transform);

            _enemyUnitPanel.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.LowerRight;
            detectedUnit.GetComponent<Unit>().unitProfilePanel = _enemyUnitPanel;
            _enemyUnitPanel.GetComponent<UnitProfileController>().ApplyProfileChanges(detectedUnit);

        }
        OnClickedOnTile(this);
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
            activePlayerUnit.unitProfilePanel.GetComponent<UnitProfileController>().activeCharacterAttackPower.text = activePlayerUnit.unitAttackPower.ToString();
            activePlayerUnit.unitProfilePanel.GetComponent<UnitProfileController>().activeCharacterMagicPower.text = activePlayerUnit.unitMagicPower.ToString();
        }
    }
    public void OnEndDrag(PointerEventData eventData)
    {

    }

    private void ApplyProfileChangesWrapper()
    {
        if (_enemyUnitPanel == null)
            return;
        _enemyUnitPanel.GetComponent<UnitProfileController>().ApplyProfileChanges(detectedUnit);
    }
}