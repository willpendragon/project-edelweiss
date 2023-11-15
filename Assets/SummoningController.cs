using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SummoningController : MonoBehaviour
{
    public EnemyTurnManager enemyTurnManager;
    public GameObject currentDeity;
    public Transform deitySummoningSpot;
    public Slider deityPowerLoadingBarSlider;
    public ParticleSystem deityAttackFeedback;
    [SerializeField] float deityAttackFeedbackTimeToDeactivation;
    public Button useDeityAttack;
    public bool deityPowerLoadingBarSliderIsActive;

    public void OnEnable()
    {
        BattleManager.OnChargeDeityPowerLoadingBar += IncreaseDeityPowerLoadingBar;
    }
    public void OnDisable()
    {
        BattleManager.OnChargeDeityPowerLoadingBar -= IncreaseDeityPowerLoadingBar;
    }
    public void Start()
    {
        if (currentDeity != null)
        {
            UpdateDeityPowerLoadingBar();
        }
    }
    public void SummonDeityOnBattlefield()
    {
        if (currentDeity != null && enemyTurnManager.deity == null)
        {
            Debug.Log("Summon Deity on Battlefield");
            GameObject deityInstance = Instantiate(currentDeity, deitySummoningSpot);
            deityPowerLoadingBarSliderIsActive = true;
        }
        else
        {
            Debug.Log("Unable to Summon Deity on Battlefield");
        }
    }
    public void UpdateDeityPowerLoadingBar()
    {
        if (deityPowerLoadingBarSlider != null)
        {
            deityPowerLoadingBarSlider.value = 0;
            deityPowerLoadingBarSlider.maxValue = 3;
        }
    }
    public void UseDeityAttack()
    {
        //Play Deity Attack Feedback
        deityAttackFeedback.Play();
        Debug.Log("Unleashing Deity Attack on Battlefield");
        //Apply Damage to Enemies
        //Reset Deity Power Loading Bar
        deityPowerLoadingBarSlider.value = 0;
        StartCoroutine("StopDeityAttackFeedback");
    }
    public void IncreaseDeityPowerLoadingBar()
    {
        if (deityPowerLoadingBarSliderIsActive == true)
        {
            deityPowerLoadingBarSlider.value += 1;
        }
    }
    IEnumerator StopDeityAttackFeedback()
    {
        yield return new WaitForSeconds(deityAttackFeedbackTimeToDeactivation);
        deityAttackFeedback.Stop();
    }
    public void CheckIfPowerLoadingBarSliderIsFull()
    {
        if (deityPowerLoadingBarSlider.value == deityPowerLoadingBarSlider.maxValue)
        {
            //Execute "Deity Attack is ready" feedback
            useDeityAttack.interactable = true;
        }
    }
}