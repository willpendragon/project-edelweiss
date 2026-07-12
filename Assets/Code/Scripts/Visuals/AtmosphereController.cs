using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AtmosphereController : MonoBehaviour
{
    [SerializeField] GameObject fogGameObject;
    [SerializeField] Camera mainCamera;
    private bool _bloodMoonActive;

    [SerializeField] UnityEngine.Rendering.VolumeProfile _bloodMoonVolumeProfile;
    void Start()
    {
        if (BattleTypeController.Instance.currentBattleType == BattleTypeController.BattleType.BattleWithDeity)
        {
            fogGameObject.SetActive(false);
            mainCamera.clearFlags = CameraClearFlags.Skybox;
        }

        BloodMoonManager bloodMoonManager = BloodMoonManager.Instance;
        if (bloodMoonManager != null && bloodMoonManager.IsBloodMoonActive)
        {
            UseBloodMoonVolume();
        }

    }

    public void UpdateGlobalVolume(MapData currentMapData)
    {
        if (_bloodMoonActive == true)
        {
            UseBloodMoonVolume();
            return;
        }
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

    private void UseBloodMoonVolume()
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
        sceneVolume.sharedProfile = _bloodMoonVolumeProfile;
    }
}
