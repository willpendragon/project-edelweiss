using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtmosphereController : MonoBehaviour
{
    [SerializeField] GameObject fogGameObject;
    [SerializeField] Camera mainCamera;
    void Start()
    {
        if (BattleTypeController.Instance.currentBattleType == BattleTypeController.BattleType.BattleWithDeity)
        {
            fogGameObject.SetActive(false);
            mainCamera.clearFlags = CameraClearFlags.Skybox;
        }
    }
}
