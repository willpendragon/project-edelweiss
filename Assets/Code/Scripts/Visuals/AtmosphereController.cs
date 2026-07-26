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
        // Different atmospheres apply to different types of battle.
        if (BattleTypeController.Instance.currentBattleType == BattleTypeController.BattleType.BattleWithDeity)
        {
            fogGameObject.SetActive(false);
            mainCamera.clearFlags = CameraClearFlags.Skybox;
        }

        // Quick-fix: deactivates fog in Puzzle Battles to give a dungeon-like impression.
        // Can't be applied to all puzzle battles but it's alright for the time being.
        if (BattleTypeController.Instance.currentBattleType == BattleTypeController.BattleType.PuzzleBattle)
        {
            fogGameObject.SetActive(false);
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = Color.black;
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
