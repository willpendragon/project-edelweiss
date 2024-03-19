using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeitySpawner : MonoBehaviour
{
    [SerializeField] GameObject[] spawnableDeities;
    [SerializeField] Transform deitySpawnPosition;
    [SerializeField] DeityAchievementsController deityAchievementsController;
    [SerializeField] BattleManager battleManager;

    public GameObject deityBossFightSpot;

    public Deity currentUnboundDeity;
    // Start is called before the first frame update
    public GameObject deityHealthBar;
    void Start()
    {
        if (battleManager.currentBattleType == BattleType.regularBattle)
        {
            var deityRoll = Random.Range(0, 7);
            if (deityRoll <= 6 && deityRoll >= 3)
            {
                DeitySelector();
                Debug.Log("Rolled Deity arrival on battlefield");
            }
        }
    }
    public void DeitySelector()
    {
        Debug.Log("Rolling which Deity will appear");
        int deityIndex = Random.Range(0, spawnableDeities.Length);
        GameObject spawningDeity = spawnableDeities[deityIndex];
        Instantiate(spawningDeity, deitySpawnPosition);
        GameObject.FindGameObjectWithTag("BattleManager").GetComponent<BattleManager>().deity = spawningDeity.GetComponent<Deity>();
    }
    void CreateDeityHealthBar(GameObject spawnedUnboundDeity)
    {
        Deity currentSpawnedUnboundDeity = spawnedUnboundDeity.GetComponent<Deity>();

        GameObject battleInterfaceCanvasGO = GameObject.FindGameObjectWithTag("BattleInterfaceCanvas");
        GameObject deityHealthBarInstance = Instantiate(deityHealthBar, battleInterfaceCanvasGO.transform);
        Slider deityHPSlider = deityHealthBarInstance.GetComponentInChildren<Slider>();
        currentSpawnedUnboundDeity.deityHealthBar = deityHealthBarInstance;

        Unit currentSpawnedUnboundDeityUnit = currentSpawnedUnboundDeity.GetComponentInChildren<Unit>();

        deityHPSlider.maxValue = currentSpawnedUnboundDeityUnit.unitMaxHealthPoints;
        deityHPSlider.value = currentSpawnedUnboundDeityUnit.unitHealthPoints;
        deityHPSlider.GetComponentInChildren<TextMeshProUGUI>().text = currentSpawnedUnboundDeityUnit.unitMaxHealthPoints.ToString();

        Debug.Log("Spawning Deity Health Bar");
    }
    public void UpdateDeityHealthBar()
    {

    }
}
