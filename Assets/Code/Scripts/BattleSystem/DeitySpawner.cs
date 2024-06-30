using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class DeitySpawner : MonoBehaviour
{
    [SerializeField] GameObject[] spawnableDeities;
    [SerializeField] Transform deitySpawnPosition;
    [SerializeField] DeityAchievementsController deityAchievementsController;
    [SerializeField] BattleManager battleManager;

    private System.Random localRandom = new System.Random(); // Local random number generator


    public Deity currentUnboundDeity;
    // Start is called before the first frame update
    public GameObject deityHealthBar;
    void Start()
    {
        if (battleManager.currentBattleType == BattleType.regularBattle)
        {
            var deityRoll = localRandom.Next(0, 7); // Use System.Random for roll
            if (deityRoll >= 3 && deityRoll <= 6)
            {
                DeitySelector();
                Debug.Log("Rolled Deity arrival on battlefield");
            }
        }
    }
    public void DeitySelector()
    {
        Debug.Log("Rolling which Deity will appear");
        int deityIndex = localRandom.Next(0, spawnableDeities.Length); // Use System.Random for deity selection
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

    public void InitiateBattleWithDeity(GameObject unlockedDeity)
    {
        //Unlocks Anguana as an Unbound Entity
        //0 is a magic number, remove it later
        Debug.Log("Unbound Anguana Unlocked");
        unlockedDeity.GetComponent<Unit>().startingXCoordinate = 5;
        unlockedDeity.GetComponent<Unit>().startingYCoordinate = 9;
        GameObject unboundDeity = Instantiate(unlockedDeity);
        Debug.Log("Moving Unbound Deity to Starting Position");
        if (unboundDeity != null)
        {
            Debug.Log("Start of Summon Deity on Battlefield");
            unboundDeity.GetComponent<Unit>().MoveUnit(5, 9);
            //Beware, Magic Numbers
            TileController deitySpawningTile = GridManager.Instance.GetTileControllerInstance(5, 9);

            unboundDeity.GetComponent<Unit>().ownedTile = deitySpawningTile;

            //Deity occupies multiple tiles
            int summoningRange = 2;
            foreach (var deitySpawningZoneTile in GameObject.FindGameObjectWithTag("GridMovementController").GetComponent<GridMovementController>().GetMultipleTiles(deitySpawningTile, summoningRange))
            {
                deitySpawningZoneTile.currentSingleTileCondition = SingleTileCondition.occupiedByDeity;
                deitySpawningZoneTile.GetComponentInChildren<SpriteRenderer>().material.color = Color.magenta;
                deitySpawningZoneTile.detectedUnit = unboundDeity;
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
    }
}
