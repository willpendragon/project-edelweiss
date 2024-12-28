using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] BattleManager battleManager;
    [SerializeField] Animator cameraAnimator;
    void Start()
    {
        if (BattleTypeController.Instance.currentBattleType == BattleTypeController.BattleType.BattleWithDeity)
        {
            cameraAnimator.SetTrigger("DeityEntryCamera");
        }
    }
}
