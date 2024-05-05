using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using DG.Tweening;
using Unity.VisualScripting;
using System;

public class DeityFeedbackController : MonoBehaviour
{
    public Volume battleLevelVolume;
    private ColorAdjustments colorAdjustments;

    public GameObject unitSpotLight;
    public GameObject DeitySpotlight;
    public Light directionalLight;

    public GameObject prayingVFX;

    public void OnEnable()
    {
        DeityPowerController.OnPlayerUnitPraying += PlayerUnitPrayingFeedback;
    }

    public void OnDisable()
    {
        DeityPowerController.OnPlayerUnitPraying -= PlayerUnitPrayingFeedback;
    }

    public void PlayerUnitPrayingFeedback()
    {
        float playerUnitPrayerDuration = 1.5f;
        float postExposureReduction = -1.8f;

        if (!battleLevelVolume.profile.TryGet<ColorAdjustments>(out colorAdjustments))
        {
            return;
        }
        ChangeLevelPostExposure(postExposureReduction, playerUnitPrayerDuration);

        Transform activePlayerUnitTransform = GameObject.FindGameObjectWithTag("ActivePlayerUnit").transform;

        // Spawn Player Spotlight
        GameObject unitSpotLightInstance = Instantiate(unitSpotLight, activePlayerUnitTransform);

        // Activate Prayer VFX
        GameObject prayingVFXInstance = Instantiate(prayingVFX, activePlayerUnitTransform);

        //Destroy Player Spotlight and Prayer VFX Instances

        Destroy(unitSpotLightInstance, playerUnitPrayerDuration);
        Destroy(prayingVFXInstance, playerUnitPrayerDuration);

        StartCoroutine(RestoreLighting(playerUnitPrayerDuration));
    }

    private void ChangeLevelPostExposure(float postExposureReduction, float playerUnitPrayerDuration)
    {
        colorAdjustments.postExposure.Override(postExposureReduction);
        DOTween.To(() => colorAdjustments.postExposure.value, x => colorAdjustments.postExposure.Override(x), postExposureReduction, playerUnitPrayerDuration);
        float lightsOffDuration = 0.1f;
        directionalLight.DOIntensity(0, lightsOffDuration);
    }

    IEnumerator RestoreLighting(float timeToRestore)
    {
        float postExposureOriginalValue = 0.36f;
        float lightsOnDuration = 0.1f;
        float directionalLightIntensityOriginalValue = 2.4f;
        yield return new WaitForSeconds(timeToRestore);
        ChangeLevelPostExposure(postExposureOriginalValue, timeToRestore);
        directionalLight.DOIntensity(directionalLightIntensityOriginalValue, lightsOnDuration);

        // Play Feedback on Deity

    }
}

