using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

    public void UpdateGlobalVolume(MapData currentMapData)
    {
        UnityEngine.Rendering.Volume sceneVolume =
            FindObjectsOfType<UnityEngine.Rendering.Volume>().FirstOrDefault(v => v.isGlobal);

        if (sceneVolume == null)
        {
            GameObject volumeObj = new GameObject("MapGlobalVolume");
            sceneVolume = volumeObj.AddComponent<UnityEngine.Rendering.Volume>();
            sceneVolume.isGlobal = true;
            volumeObj.transform.SetParent(this.transform);
        }

        sceneVolume.sharedProfile = currentMapData.globalVolumeProfile;
    }
}
