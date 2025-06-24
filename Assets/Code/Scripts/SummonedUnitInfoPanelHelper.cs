using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SummonedUnitInfoPanelHelper : MonoBehaviour
// Should be called PrayToSummonedUnitController
{
    [SerializeField] TextMeshProUGUI summonedUnitNameText;
    [SerializeField] Slider prayerBuffThresholdSlider;
    public bool deityPrayerPowerBarFull;
    public delegate void PlayerPrayer();
    public static event PlayerPrayer OnPlayerPrayer;

    private void OnEnable()
    {
        BattleInterface.Instance.summonedUnitInfoPanelHelper = this;
        DeityPowerController.OnPlayerUnitPraying += IncreaseDeityPrayerPowerSliderValue;
    }
    private void OnDisable()
    {
        BattleInterface.Instance.summonedUnitInfoPanelHelper = null;
        DeityPowerController.OnPlayerUnitPraying -= IncreaseDeityPrayerPowerSliderValue;
    }
    public void SetSummonedUnitInfoPanelValues(string summonedUnitName, float deityBuffThreshold, float deityPrayerPower)
    {
        summonedUnitNameText.text = summonedUnitName;
        prayerBuffThresholdSlider.value = deityPrayerPower;
        prayerBuffThresholdSlider.maxValue = deityBuffThreshold;
    }
    private void IncreaseDeityPrayerPowerSliderValue()
    {
        prayerBuffThresholdSlider.value++;
    }
    public void PrayDeity()
    {
        Unit currentActivePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();
        currentActivePlayerUnit.unitOpportunityPoints--;
        UpdateActivePlayerUnitProfile(currentActivePlayerUnit);
        OnPlayerPrayer();
        // Plays the Prayer's SFX
        StageMoodController.Instance.ActivateDarkness();
        float darknessResetWaitTime = 1.5f;
        StageMoodController.Instance.StartResetDarkness(darknessResetWaitTime);
        currentActivePlayerUnit.battleFeedbackController.PlayPrayerSFX.Invoke();
        Debug.Log("The Current Active Unit is praying to the Linked Deity");
    }
    public void CheckDeityPrayerPower()
    {
        if (prayerBuffThresholdSlider.value >= prayerBuffThresholdSlider.maxValue)
        {
            ExecuteSummonedUnitSequence();
        }
    }
    public void ExecuteSummonedUnitSequence()
    {
        Unit currentActivePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();
        Deity summonedLinkedDeity = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>().summonedLinkedDeity;
        string summonedLinkedDeityUnitName = summonedLinkedDeity.gameObject.GetComponent<Unit>().unitTemplate.unitName;
        string currentActivePlayerUnitName = currentActivePlayerUnit.gameObject.GetComponent<Unit>().unitTemplate.unitName;
        BattleInterface.Instance.SetSummonEffectNameOnNotificationPanel(summonedLinkedDeityUnitName, currentActivePlayerUnitName);
        //currentActivePlayerUnit.unitOpportunityPoints--;
        UpdateActivePlayerUnitProfile(currentActivePlayerUnit);
        summonedLinkedDeity.summoningBehaviour.ExecuteBehavior(summonedLinkedDeity);
        Debug.Log("The Deity fulfilled the Current Active User's prayer.");
        ResetSummon(currentActivePlayerUnit, summonedLinkedDeity);
    }
    // This method is called by the Slider at each value change.
    public void UpdateActivePlayerUnitProfile(Unit activePlayerUnit)
    {
        activePlayerUnit.unitProfilePanel.GetComponent<PlayerProfileController>().UpdateActivePlayerProfile(activePlayerUnit);
        Debug.Log("Updating OP points after using Prayer");
    }
    private void ResetSummon(Unit currentActivePlayerUnit, Deity summonedLinkedDeity)
    {
        SummoningUIController currentActivePlayerUnitSummoningUIController = currentActivePlayerUnit.gameObject.GetComponent<SummoningUIController>();
        currentActivePlayerUnitSummoningUIController.currentSummonPhase = SummoningUIController.SummonPhase.summoning;
        currentActivePlayerUnitSummoningUIController.currentButton.GetComponentInChildren<Text>().text = "Summon";

        PlayDespawnTween(summonedLinkedDeity.gameObject);

        Destroy(summonedLinkedDeity.gameObject, 3);
        Debug.Log("Summoned Deity disappeared from the Battlefield.");
        Destroy(this.gameObject, 3);
        Debug.Log("Destroyed Summoned Deity Panel");
        ResetTileControllersAfterDeitySummon();
    }
    private void PlayDespawnTween(GameObject deityGO)
    {
        Sequence despawnSequence = DOTween.Sequence();

        // Quick scale shake to simulate a flicker or instability
        despawnSequence.Append(deityGO.transform.DOShakeScale(0.2f, strength: 0.2f, vibrato: 10)
            .SetEase(Ease.OutQuad));

        // Instantly shrink (like vanishing)
        despawnSequence.Append(deityGO.transform.DOScale(Vector3.zero, 0.1f)
            .SetEase(Ease.InFlash));

        // Very quick up/down movement
        despawnSequence.Join(deityGO.transform.DOMoveY(deityGO.transform.position.y + 0.2f, 0.1f));

        despawnSequence.OnComplete(() =>
        {
            Destroy(deityGO);
            Debug.Log("Summoned Deity teleported away.");
        });
    }
    // I should centralize this logic.
    public void ResetTileControllersAfterDeitySummon()
    {
        foreach (var tile in GridManager.Instance.gridTileControllers)
        {
            tile.tileShaderController.AnimateFadeHeight(0, 0.2f, Color.white);
            Debug.Log("Reset Tile Glow");
            if (tile.currentSingleTileCondition == SingleTileCondition.occupiedByDeity)
            {
                tile.currentSingleTileCondition = SingleTileCondition.free;
            }
        }
    }
}