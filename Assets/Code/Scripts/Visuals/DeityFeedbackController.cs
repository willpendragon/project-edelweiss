using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using DG.Tweening;

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

        GameObject activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit");
        BattleInterface.Instance.SetSpellNameOnNotificationPanel("Prayer", activePlayerUnit.transform.gameObject.GetComponent<Unit>().unitTemplate.unitName);

        float yOffset = 3.5f;

        // Calculate the new spawn position with the Y offset
        Vector3 unitPrayingVFXPosition = activePlayerUnit.transform.position + new Vector3(0, yOffset, 0);

        GameObject unitPrayingVFX = Instantiate(Resources.Load<GameObject>("UnitPrayingVFX"), unitPrayingVFXPosition, Quaternion.identity);
        //unitPrayingVFX.GetComponent<Animator>().SetTrigger("unitIsPraying");
        float unitPrayingVFXDestroyCountdown = 1.03f;
        Destroy(unitPrayingVFX, unitPrayingVFXDestroyCountdown);

        //float playerUnitPrayerDuration = 1.5f;
        //float postExposureReduction = -1.8f;

        //if (!battleLevelVolume.profile.TryGet<ColorAdjustments>(out colorAdjustments))
        //{
        //    return;
        //}
        //ChangeLevelPostExposure(postExposureReduction, playerUnitPrayerDuration);


        //// Spawn Player Spotlight
        //GameObject unitSpotLightInstance = Instantiate(unitSpotLight, activePlayerUnit.transform);

        //// Activate Prayer VFX
        //GameObject prayingVFXInstance = Instantiate(prayingVFX, activePlayerUnit.transform);

        ////Destroy Player Spotlight and Prayer VFX Instances

        //Destroy(unitSpotLightInstance, playerUnitPrayerDuration);
        //Destroy(prayingVFXInstance, playerUnitPrayerDuration);

        //StartCoroutine(RestoreLighting(playerUnitPrayerDuration));
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
        Deity linkedDeity = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>().linkedDeity;

        GameObject prayingDeityVFXInstance = Instantiate(prayingVFX, linkedDeity.transform);
        Destroy(prayingDeityVFXInstance, timeToRestore);


    }
}

