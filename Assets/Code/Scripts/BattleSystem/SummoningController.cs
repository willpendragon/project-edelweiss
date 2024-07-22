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
    public GameObject summonedLinkedDeityInstance;

    public delegate void SummoningRitual();
    public static event SummoningRitual OnSummoningRitual;

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
        if (linkedDeity != null && currentActivePlayerUnit.GetComponent<Unit>().unitManaPoints >= linkedDeity.summoningPrice)
        {
            int summoningRange = 2;

            foreach (var deitySpawningZoneTile in GameObject.FindGameObjectWithTag("GridMovementController").GetComponent<GridMovementController>().GetMultipleTiles(summonAreaCenterTile, summoningRange))
            {
                deitySpawningZoneTile.currentSingleTileCondition = SingleTileCondition.occupiedByDeity;
            }
            currentActivePlayerUnit.GetComponent<Unit>().unitManaPoints -= linkedDeity.summoningPrice;

            Debug.Log("Summoned Unit's Linked Deity on Battlefield");

            var summonPosition = summonAreaCenterTile.transform.position + new Vector3(0, 3, 0);
            summonedLinkedDeityInstance = Instantiate(linkedDeity.gameObject, summonPosition, Quaternion.identity);
            summonedLinkedDeityInstance.transform.localScale = new Vector3(2, 2, 2);

        }
        else
        {
            Debug.Log("Unable to Summon Deity on Battlefield");
        }
    }


}
