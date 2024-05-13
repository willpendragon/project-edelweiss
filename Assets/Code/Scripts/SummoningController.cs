using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    public TileController summonAreaCenterTile;

    public delegate void SummoningRitual();
    public static event SummoningRitual OnSummoningRitual;

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
    public void SetSummoningCenter(TileController summonAreaCenter)
    {
        summonAreaCenterTile = summonAreaCenter;
    }

    public void StartSummoningRitual()
    {
        OnSummoningRitual();
    }
    public void SummonDeityOnBattlefield()
    {
        GameObject currentActivePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit");
        Deity linkedDeity = currentActivePlayerUnit.GetComponent<Unit>().linkedDeity;
        if (linkedDeity != null)
        {
            Debug.Log("Start of Summon Deity on Battlefield");
            foreach (var deitySpawningZoneTile in GameObject.FindGameObjectWithTag("GridMovementController").GetComponent<GridMovementController>().GetMultipleTiles(summonAreaCenterTile))
            {
                deitySpawningZoneTile.currentSingleTileCondition = SingleTileCondition.occupiedByDeity;
            }
            currentActivePlayerUnit.GetComponent<Unit>().unitManaPoints -= linkedDeity.summoningPrice;

            Debug.Log("Summoned Deity on Battlefield");

            var summonPosition = summonAreaCenterTile.transform.position + new Vector3(0, 3, 0);
            GameObject deityInstance = Instantiate(linkedDeity.gameObject, summonPosition, Quaternion.identity);
            deityInstance.transform.localScale = new Vector3(2, 2, 2);

        }
        else
        {
            Debug.Log("Unable to Summon Deity on Battlefield");
        }
    }

    // Obsolete, consider using again the Deity Attack

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

        //Remember to Apply Damage to Enemies

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