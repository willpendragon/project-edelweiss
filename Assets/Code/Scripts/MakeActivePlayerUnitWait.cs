using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeActivePlayerUnitWait : MonoBehaviour
{
    public void SetActivePlayerUnitToWaitingMode()
    {
        GridManager.Instance.currentPlayerUnit.GetComponent<UnitSelectionController>().currentUnitSelectionStatus = UnitSelectionController.UnitSelectionStatus.unitWaiting;
        GridManager.Instance.currentPlayerUnit.GetComponent<UnitSelectionController>().StopUnitAction();
        //GridManager.Instance.SwitchToCharacterSelectionMove();
        Destroy(GameObject.FindGameObjectWithTag("TargetedEnemyUnitProfile"));
        Destroy(GameObject.FindGameObjectWithTag("EnemyTargetIcon"));

    }
}