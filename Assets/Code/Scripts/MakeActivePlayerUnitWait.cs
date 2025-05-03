using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeActivePlayerUnitWait : MonoBehaviour
{
    public delegate void PlayerWaiting();
    public static event PlayerWaiting OnPlayerWaiting;
    public void SetActivePlayerUnitToWaitingMode()
    {
        DestroyMagnet();
        DeactivateTrapSelection();

        GridManager.Instance.currentPlayerUnit.GetComponent<UnitSelectionController>().currentUnitSelectionStatus = UnitSelectionController.UnitSelectionStatus.unitWaiting;
        GridManager.Instance.currentPlayerUnit.GetComponent<UnitSelectionController>().StopUnitAction();

        Destroy(GameObject.FindGameObjectWithTag("TargetedEnemyUnitProfile"));
        Destroy(GameObject.FindGameObjectWithTag("EnemyTargetIcon"));

        foreach (var tile in GridManager.Instance.gridTileControllers)
        {
            tile.currentPlayerAction = new SelectUnitPlayerAction();
        }

        BattleInterface.Instance.movesContainer.transform.localScale = new Vector3(0, 0, 0);

        ResetTileControllersColours();
        ResetTileControllersGlow();
        ClearPath();
        OnPlayerWaiting();
        UnitProfilesController.Instance.DestroyEnemyUnitPanel();

        //BattleManager.Instance.GetComponent<TurnController>().GameOverCheck();
    }
    void DestroyMagnet()
    {
        Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();
        if (activePlayerUnit != null && activePlayerUnit.hasHookshot == true)
        {
            MagnetHelper magnetHelper = activePlayerUnit.gameObject.GetComponentInChildren<MagnetHelper>();
            magnetHelper.DestroyMagnet();
        }
    }

    void DeactivateTrapSelection()
    {
        GridManager.Instance.RemoveTrapSelection();
    }

    public void ResetTileControllersColours()
    {
        foreach (var tile in GridManager.Instance.gridTileControllers)
        {
            Debug.Log("Resetting Tile Colours");
            tile.GetComponentInChildren<SpriteRenderer>().color = Color.white;
        }
    }
    public void ResetTileControllersGlow()
    {
        foreach (var tile in GridManager.Instance.gridTileControllers)
        {
            if (tile.currentSingleTileCondition != SingleTileCondition.occupiedByDeity)
            {
                Debug.Log("Resetting Tile Glow");
                tile.tileShaderController.AnimateFadeHeight(0, 0.2f, Color.white);
            }
        }
    }
    public void ClearPath()
    {
        LineRenderer lineRenderer = GridManager.Instance.GetLineRenderer();
        lineRenderer.positionCount = 0;
    }
}