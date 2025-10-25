using System.Collections;
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
        BattleInterface.Instance.SetBattleNotification($"{activePlayerUnit.transform.gameObject.GetComponent<Unit>().unitTemplate.unitName} used Prayer");

        float yOffset = 3.5f;

        // Calculate the new spawn position with the Y offset
        Vector3 unitPrayingVFXPosition = activePlayerUnit.transform.position + new Vector3(0, yOffset, 0);

        GameObject unitPrayingVFX = Instantiate(Resources.Load<GameObject>("UnitPrayingVFX"), unitPrayingVFXPosition, Quaternion.identity);
        float unitPrayingVFXDestroyCountdown = 1.03f;
        Destroy(unitPrayingVFX, unitPrayingVFXDestroyCountdown);
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

