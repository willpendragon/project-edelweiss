using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeActivePlayerUnitWait : MonoBehaviour
{
    public delegate void PlayerWaiting();
    public static event PlayerWaiting OnPlayerWaiting;
    public void SetActivePlayerUnitToWaitingMode()
    {
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

        BattleManager.Instance.GetComponent<TurnController>().GameOverCheck();
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
            Debug.Log("Resetting Tile Glow");
            tile.tileShaderController.AnimateFadeHeight(0, 0.2f, Color.white);
        }
    }

    public void ClearPath()
    {
        LineRenderer lineRenderer = GridManager.Instance.GetLineRenderer();
        lineRenderer.positionCount = 0;
    }
}