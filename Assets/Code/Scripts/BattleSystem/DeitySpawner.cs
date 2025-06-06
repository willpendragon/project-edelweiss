using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class DeitySpawner : MonoBehaviour
{
    [SerializeField] GameObject[] spawnableDeities;
    [SerializeField] Transform deitySpawnPosition;
    [SerializeField] DeityAchievementsController deityAchievementsController;
    [SerializeField] BattleManager battleManager;
    [SerializeField] GameObject deityObelisk;
    [SerializeField] GameObject deityObeliskSpawningPoint;

    private GameObject deityHealthBarInstance;


    private System.Random localRandom = new System.Random(); // Local random number generator

    public Deity currentUnboundDeity;
    public GameObject deityHealthBar;

    void Start()
    {
        if (BattleTypeController.Instance.currentBattleType == BattleTypeController.BattleType.RegularBattle)
        {
            string sceneName = SceneManager.GetActiveScene().name;

            // This behaviour allows to spawn Tomodachick as a Deity in the Tutorial Battle.
            if (sceneName == "battle_tutorial")
            {
                GameObject spawningDeity = spawnableDeities[0];
                GameObject deityOnBattlefield = Instantiate(spawningDeity, deitySpawnPosition.position, Quaternion.identity);

                BattleManager.Instance.deity = deityOnBattlefield.GetComponent<Deity>();
            }
            else
            {
                int deityRollMinRange = 0;
                int deityRollMaxRange = 7;
                var deityRoll = localRandom.Next(deityRollMinRange, deityRollMaxRange);

                int deityRollFirstThreshold = 3;
                int deityRollSecondThreshold = 6;
                if (deityRoll >= deityRollFirstThreshold && deityRoll <= deityRollSecondThreshold)
                {
                    DeitySelector();
                    Debug.Log("Rolled Deity arrival on battlefield");
                }
            }
        }
        else if (BattleTypeController.Instance.currentBattleType == BattleTypeController.BattleType.BattleWithDeity)
        {
            if (deityHealthBarInstance != null)
            {
                PopulateDeityHealthBar();
            }
        }



    }
    public void DeitySelector()
    {
        Debug.Log("Rolling which Deity will appear");
        int deityIndex = localRandom.Next(0, spawnableDeities.Length); // Use System.Random for Deity selection
        Debug.Log($"Deity Index: {deityIndex} - {spawnableDeities[deityIndex].name}");

        GameObject spawningDeity = spawnableDeities[deityIndex];
        GameObject deityOnBattlefield = Instantiate(spawningDeity, deitySpawnPosition.position, Quaternion.identity);

        BattleManager.Instance.deity = deityOnBattlefield.GetComponent<Deity>();

        // Set initial scale and transparency
        deityOnBattlefield.transform.localScale = Vector3.zero;
        MeshRenderer[] meshRenderers = deityOnBattlefield.GetComponentsInChildren<MeshRenderer>();
        foreach (var renderer in meshRenderers)
        {
            foreach (var material in renderer.materials)
            {
                Color initialColor = material.color;
                initialColor.a = 0f; // Make the deity almost invisible
                material.color = initialColor;
            }
        }

        // Define the sequence of animations
        Sequence deitySequence = DOTween.Sequence();

        // Step 1: Fade in and scale up
        foreach (var renderer in meshRenderers)
        {
            foreach (var material in renderer.materials)
            {
                deitySequence.Join(material.DOFade(1f, 1f).SetEase(Ease.InQuad));
            }
        }
        deitySequence.Join(deityOnBattlefield.transform.DOScale(new Vector3(7.5f, 7.5f, 7.5f), 1f).SetEase(Ease.OutQuad));

        // Step 2: Scale down to the final size
        deitySequence.Append(deityOnBattlefield.transform.DOScale(new Vector3(6.667861f, 6.667861f, 6.667861f), 0.5f).SetEase(Ease.InOutQuad));

        // Play the sequence
        deitySequence.Play();
    }
    public void InitiateBattleWithDeity(GameObject unlockedDeity)
    {
        //Unlocks Deity as an Unbound Entity
        Debug.Log("Unbound Deity Unlocked");

        int unlockedDeityStartingTileXCoordinate = 5;
        int unlockedDeityStartingTileYCoordinate = 5;

        unlockedDeity.GetComponent<Unit>().startingXCoordinate = unlockedDeityStartingTileXCoordinate;
        unlockedDeity.GetComponent<Unit>().startingYCoordinate = unlockedDeityStartingTileYCoordinate;
        GameObject unboundDeity = Instantiate(unlockedDeity);
        Debug.Log("Moving Unbound Deity to Starting Position");

        if (unboundDeity != null)
        {
            Debug.Log("Start of Summon Deity on Battlefield");
            int deityTilePositionX = 5;
            int deityTilePositionY = 5;
            unboundDeity.GetComponent<Unit>().MoveUnit(deityTilePositionX, deityTilePositionY, false);
            TileController firstDeitySpawningTile = GridManager.Instance.GetTileControllerInstance(deityTilePositionX, deityTilePositionY);

            unboundDeity.GetComponent<Unit>().ownedTile = firstDeitySpawningTile;
            GameObject deityObeliskInstance = Instantiate(deityObelisk, deityObeliskSpawningPoint.transform);

            //Deity occupies multiple tiles
            //int summoningRange = 1;
            GridMovementController gridMovementController = GameObject.FindGameObjectWithTag("GridMovementController").GetComponent<GridMovementController>();
            firstDeitySpawningTile.currentSingleTileCondition = SingleTileCondition.occupiedByDeity;
            //GameObject deityEnergyVFXPrefab = Resources.Load<GameObject>("DeityEnergy");
            //GameObject deityEnergyVFXPrefabInstance = Instantiate(deityEnergyVFXPrefab, firstDeitySpawningTile.transform.position, Quaternion.identity);
            firstDeitySpawningTile.detectedUnit = unboundDeity;
            currentUnboundDeity = unboundDeity.GetComponent<Deity>();
            Debug.Log("Deity occupies Tile");
        }

        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            Destroy(enemy);
        }
        unboundDeity.gameObject.tag = "Enemy";
        CreateDeityHealthBar(unboundDeity);
    }

    void CreateDeityHealthBar(GameObject spawnedUnboundDeity)
    {
        currentUnboundDeity = spawnedUnboundDeity.GetComponent<Deity>();

        GameObject battleInterfaceCanvasGO = GameObject.FindGameObjectWithTag("BattleInterfaceCanvas");
        deityHealthBarInstance = Instantiate(deityHealthBar, battleInterfaceCanvasGO.transform);
        currentUnboundDeity.deityHealthBar = deityHealthBarInstance;
        Debug.Log("Spawning Deity Health Bar");
    }

    void PopulateDeityHealthBar()
    {
        Slider deityHPSlider = deityHealthBarInstance.GetComponentInChildren<Slider>();

        Unit currentUnboundDeityUnit = currentUnboundDeity.gameObject.GetComponent<Unit>();

        deityHPSlider.maxValue = currentUnboundDeityUnit.unitTemplate.unitMaxHealthPoints;
        deityHPSlider.value = currentUnboundDeityUnit.GetComponent<Unit>().unitTemplate.unitHealthPoints; ;
        deityHPSlider.GetComponentInChildren<TextMeshProUGUI>().text = currentUnboundDeityUnit.unitTemplate.unitMaxHealthPoints.ToString();
    }

    public void MoveObeliskOnGridMap()
    {
        deityObeliskSpawningPoint.transform.position = currentUnboundDeity.gameObject.GetComponent<Unit>().ownedTile.gameObject.transform.position;
    }
}