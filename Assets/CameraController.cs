using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] BattleManager battleManager;
    [SerializeField] Animator cameraAnimator;
    void Start()
    {
        if (battleManager.currentBattleType == BattleType.battleWithDeity)
        {
            cameraAnimator.SetTrigger("DeityEntryCamera");
        }
    }
}
