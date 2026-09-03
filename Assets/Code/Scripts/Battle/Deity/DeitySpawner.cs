using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using ProjectEdelweiss.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeitySpawner : MonoBehaviour
{
    // Separate Obelisk Logic on another class

    [SerializeField] List<Deity> spawnableDeities;
    [SerializeField] Transform deitySpawnPosition;
    [SerializeField] DeityAchievementsController deityAchievementsController;
    [SerializeField] BattleManager battleManager;
    [SerializeField] GameObject deityObelisk;
    [SerializeField] GameObject deityObeliskSpawningPoint;
    [SerializeField] private EnemyTurnManager _enemyTurnManager;
    private GameObject _deityObeliskInstance;

    private GameObject deityHealthBarInstance;

    // Killed Deity Dictionary
    public Dictionary<string, bool> _killedDeityDictionary = new Dictionary<string, bool>();

    public GameObject DeityObelisk => _deityObeliskInstance;
    public GameObject DeityObeliskSpawningPoint => deityObeliskSpawningPoint;

    private System.Random localRandom = new System.Random(); // Local random number generator

    public Deity currentUnboundDeity;
    public GameObject deityHealthBar;

    public void OnEnable()
    {
        TurnController.OnDeityKilled += AddDeityToKilledDictionary;
    }

    public void OnDisable()
    {
        TurnController.OnDeityKilled -= AddDeityToKilledDictionary;
    }

    private void Awake()
    {
        LoadKilledDeities();
    }

    private void Start()
    {
        // Deity Status Check
        UpdateSpawnableDeities();
        if (BattleTypeController.Instance != null && BattleTypeController.Instance.currentBattleType == BattleTypeController.BattleType.RegularBattle)
        {
            // OVERRIDE: If forced by the Roaming Deity, spawn it directly and skip random chance
            if (BattleTypeController.isForcedRoamingDeity && BattleTypeController.forcedRoamingDeityPrefab != null)
            {
                Debug.Log($"Forcing Roaming Deity: {BattleTypeController.forcedRoamingDeityPrefab.name}");
                SpawnDeity(BattleTypeController.forcedRoamingDeityPrefab);

                // Clear state for future battles
                BattleTypeController.isForcedRoamingDeity = false;
                BattleTypeController.forcedRoamingDeityPrefab = null;
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
                }
            }

            // Hides the Deity Health Bar when not necessary.
            if (BattleManager.Instance.deity == null)
                return;
            var healthBar = BattleManager.Instance.deity.GetComponentInChildren<DeityHealthBar>();
            healthBar.HideHealthBar();
        }
        else if (BattleTypeController.Instance != null && BattleTypeController.Instance.currentBattleType == BattleTypeController.BattleType.BattleWithDeity)
        {
            if (deityHealthBarInstance != null)
            {
                PopulateDeityHealthBar();
            }
        }
        else if (BattleTypeController.Instance != null && BattleTypeController.Instance.currentBattleType == BattleTypeController.BattleType.PuzzleBattle)
        {
            GameObject deityObj = GameObject.FindGameObjectWithTag(GameTags.Deity);

            if (deityObj != null) // Guard check is crucial since Deities as of now are never meant to be in a Dungeon-style Puzzle Battle.
            {
                currentUnboundDeity = deityObj.GetComponent<Deity>();
                Unit currentUnboundDeityUnit = deityObj.GetComponent<Unit>();

                if (currentUnboundDeity != null && currentUnboundDeityUnit != null && currentUnboundDeityUnit.unitTemplate != null)
                {
                    if (currentUnboundDeity.deityHealthBar != null)
                    {
                        HPSliderController sliderController = currentUnboundDeity.deityHealthBar.GetComponentInChildren<HPSliderController>();

                        if (sliderController != null && sliderController.slider != null)
                        {
                            Slider deityHPSlider = sliderController.slider;

                            deityHPSlider.maxValue = currentUnboundDeityUnit.unitTemplate.unitMaxHealthPoints;
                            deityHPSlider.value = currentUnboundDeityUnit.unitTemplate.unitHealthPoints;

                            Debug.Log($"Attempt to populate {deityObj.name} HP Slider successful.");
                        }
                    }
                }
            }
            else
            {
                Debug.Log("No Deity found in this Puzzle/Boss Battle. Skipping Deity UI setup.");
            }
        }
    }

    private void LoadKilledDeities()
    {
        GameSaveData saveData = SaveStateManager.saveData;

        _killedDeityDictionary.Clear();

        foreach (var kvp in saveData.killedDeities)
        {
            string deityName = kvp.Key;
            bool isKilled = kvp.Value;

            _killedDeityDictionary[deityName] = isKilled;
        }
    }

    private void UpdateSpawnableDeities()
    {
        // Remove deities that are killed, linked to players, or captured but unassigned
        GameSaveData saveData = SaveStateManager.saveData;

        for (int i = spawnableDeities.Count - 1; i >= 0; i--)
        {
            var deity = spawnableDeities[i];
            string deityName = deity.gameObject.GetComponent<Unit>().unitTemplate.unitName;
            string deityId = deity.Id;

            // Check if killed
            bool deityIsKilled = _killedDeityDictionary.ContainsKey(deityName) &&
                                 _killedDeityDictionary[deityName];

            // Check if captured and linked to a player
            bool deityIsLinked = saveData.unitsLinkedToDeities.ContainsValue(deityId);

            // Check if captured but unassigned
            bool deityIsUnassignedCaptured = saveData.unassignedCapturedDeities.Contains(deityId);

            if (deityIsKilled || deityIsLinked || deityIsUnassignedCaptured)
            {
                Debug.Log($"[UpdateSpawnableDeities] Removing {deityName} - Killed: {deityIsKilled}, Linked: {deityIsLinked}, Unassigned: {deityIsUnassignedCaptured}");
                spawnableDeities.RemoveAt(i);
            }
        }
    }

    public void AddDeityToKilledDictionary(Deity killedDeity)
    {
        string deityName = killedDeity.gameObject.GetComponent<Unit>().unitTemplate.unitName;
        // Save the Dictionary
        GameSaveData saveData = SaveStateManager.saveData;
        saveData.killedDeities.Add(deityName, true);
        SaveStateManager.SaveGame(saveData);
    }

    public void DeitySelector()
    {
        if (spawnableDeities == null || spawnableDeities.Count == 0)
        {
            Debug.Log("No Deities found. Typically happens if the Player killed or caught all of them.");
            return;
        }
        Debug.Log("Rolling which Deity will appear");
        int deityIndex = localRandom.Next(0, spawnableDeities.Count); // Use System.Random for Deity selection
        Debug.Log($"Deity Index: {deityIndex} - {spawnableDeities[deityIndex].name}");

        GameObject spawningDeity = spawnableDeities[deityIndex].gameObject;
        SpawnDeity(spawningDeity);
    }

    public void SpawnDeity(GameObject spawningDeity)
    {
        Vector2Int deityCoords = GameManager.Instance.GetDeityStartingCoordinates();
        Vector3 spawnWorldPos = deitySpawnPosition.position;

        // Find the tile the user painted as a DeityTile
        TileController targetDeityTile = GridManager.Instance.GetTileControllerInstance(deityCoords.x, deityCoords.y);

        if (targetDeityTile != null && targetDeityTile.tileType == TileType.DeityTile)
        {
            float finalY = targetDeityTile.transform.position.y;
            Collider col = targetDeityTile.GetComponent<Collider>();
            if (col != null) finalY = col.bounds.max.y;

            // Spawn floating directly on top of the painted DeityTile
            spawnWorldPos = new Vector3(targetDeityTile.transform.position.x, finalY,
                targetDeityTile.transform.position.z);
        }
        else
        {
            Debug.LogWarning(
                "DeitySpawner: No DeityTile found on the map. Spawning floating deity in fallback position.");
        }

        GameObject deityOnBattlefield = Instantiate(spawningDeity, spawnWorldPos, Quaternion.identity);

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

        deitySequence.Join(
            deityOnBattlefield.transform.DOScale(new Vector3(7.5f, 7.5f, 7.5f), 1f).SetEase(Ease.OutQuad));

        // Step 2: Scale down to the final size
        deitySequence.Append(deityOnBattlefield.transform.DOScale(new Vector3(6.667861f, 6.667861f, 6.667861f), 0.5f)
            .SetEase(Ease.InOutQuad));

        // Play the sequence
        deitySequence.Play();
    }

    public void InitiateBattleWithDeity(GameObject unlockedDeity)
    {
        //Unlocks Deity as an Unbound Entity
        Debug.Log($"Unlocked {unlockedDeity.GetComponent<Unit>().unitTemplate.unitName}");

        // Reset tribute modifier stacks for new deity battle
        TributeModifierTracker.Instance.ResetStacks();

        string deityName = unlockedDeity.GetComponent<Unit>().unitTemplate.unitName;

        if (DeityIsKilled(deityName))
        {
            return;
        }

        Vector2Int deityCoords = GameManager.Instance.GetDeityStartingCoordinates();

        int unlockedDeityStartingTileXCoordinate = deityCoords.x;
        int unlockedDeityStartingTileYCoordinate = deityCoords.y;

        unlockedDeity.GetComponent<Unit>().startingXCoordinate = unlockedDeityStartingTileXCoordinate;
        unlockedDeity.GetComponent<Unit>().startingYCoordinate = unlockedDeityStartingTileYCoordinate;


        // Optionally, check if a DeityTile dictates the 3D spawn position instead of relying on the static empty GameObject
        Vector3 spawnWorldPos = deitySpawnPosition.position;
        TileController firstDeitySpawningTile =
            GridManager.Instance.GetTileControllerInstance(deityCoords.x, deityCoords.y);

        if (firstDeitySpawningTile != null && firstDeitySpawningTile.tileType == TileType.DeityTile)
        {
            float finalY = firstDeitySpawningTile.transform.position.y;
            Collider col = firstDeitySpawningTile.GetComponent<Collider>();
            if (col != null) finalY = col.bounds.max.y;

            spawnWorldPos = new Vector3(firstDeitySpawningTile.transform.position.x, finalY,
                firstDeitySpawningTile.transform.position.z);
        }

        GameObject unboundDeity = Instantiate(unlockedDeity, spawnWorldPos, Quaternion.identity);
        Debug.Log($"Instantiate Unbound Deity GameObject at {spawnWorldPos}");

        if (unboundDeity != null)
        {
            Debug.Log("Start of Summon Deity on Battlefield");

            // Adjust position correctly to tile surface
            if (firstDeitySpawningTile != null)
            {
                GridManager.Instance.PlaceUnitOnTileSurface(unboundDeity, firstDeitySpawningTile);
            }

            Deity deityComponent = unboundDeity.GetComponent<Deity>();
            if (deityComponent != null && deityComponent.DeityModel != null)
            {
                deityComponent.DeityModel.transform.localPosition = new Vector3(0, 1f, 0);
            }

            unboundDeity.GetComponent<Unit>().ownedTile = firstDeitySpawningTile;
            _deityObeliskInstance = Instantiate(deityObelisk, deityObeliskSpawningPoint.transform);

            GridMovementController gridMovementController = GameObject.FindGameObjectWithTag("GridMovementController")
                .GetComponent<GridMovementController>();
            if (firstDeitySpawningTile != null)
            {
                firstDeitySpawningTile.currentSingleTileCondition = SingleTileCondition.occupiedByDeity;
                firstDeitySpawningTile.detectedUnit = unboundDeity;
            }

            currentUnboundDeity = unboundDeity.GetComponent<Deity>();
            _enemyTurnManager.deity = unboundDeity;
            MoveObeliskOnGridMap();

            Debug.Log("Deity occupies Tile");
        }

        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            Destroy(enemy);
        }

        unboundDeity.gameObject.tag = "Enemy";

    }

    public bool DeityIsUnavailable(string deityName)
    {
        // Check if killed
        if (_killedDeityDictionary.TryGetValue(deityName, out bool isKilled) && isKilled)
        {
            Debug.Log($"{deityName} has been killed, Player can't fight it");
            return true;
        }

        // Check if captured - find deity ID first
        var deity = spawnableDeities.FirstOrDefault(d =>
            d.GetComponent<Unit>().unitTemplate.unitName == deityName);

        if (deity != null)
        {
            bool isCaptured = SaveStateManager.saveData.unitsLinkedToDeities.ContainsValue(deity.Id);
            if (isCaptured)
            {
                Debug.Log($"{deityName} has been captured, Player can't fight it");
                return true;
            }
        }

        return false;
    }

    public bool DeityIsKilled(string deityName)
    {
        return DeityIsUnavailable(deityName);
    }

    private string GetDeityIdByName(string deityName)
    {
        // Find deity ID from spawnable deities or loaded data
        var deity = spawnableDeities.FirstOrDefault(d =>
            d.GetComponent<Unit>().unitTemplate.unitName == deityName);
        return deity != null ? deity.Id : null;
    }

    void PopulateDeityHealthBar()
    {
        Slider deityHPSlider = deityHealthBarInstance.GetComponentInChildren<Slider>();

        Unit currentUnboundDeityUnit = currentUnboundDeity.gameObject.GetComponent<Unit>();

        deityHPSlider.maxValue = currentUnboundDeityUnit.unitTemplate.unitMaxHealthPoints;
        deityHPSlider.value = currentUnboundDeityUnit.GetComponent<Unit>().unitTemplate.unitHealthPoints;
        ;
        deityHPSlider.GetComponentInChildren<TextMeshProUGUI>().text =
            currentUnboundDeityUnit.unitTemplate.unitMaxHealthPoints.ToString();
    }

    public void MoveObeliskOnGridMap()
    {
        deityObeliskSpawningPoint.transform.position = currentUnboundDeity.gameObject.GetComponent<Unit>().ownedTile
            .gameObject.transform.position;
    }

    public void ShowObeliskDamageFeedback()
    {
        if (_deityObeliskInstance == null) return;

        Transform target = _deityObeliskInstance.transform;

        target.DOKill();

        // Rotation shake (main impact)
        target.DOShakeRotation(
            duration: 0.25f,
            strength: new Vector3(0f, 6f, 0f),
            vibrato: 12,
            randomness: 90f,
            fadeOut: true
        );

        // Subtle position shake
        target.DOShakePosition(
            duration: 0.2f,
            strength: 1f,
            vibrato: 10,
            randomness: 90f,
            fadeOut: true
        );

        PlayDamageFlash(target);
    }

    private void PlayDamageFlash(Transform target)
    {
        var renderer = target.GetComponentInChildren<Renderer>();
        if (renderer == null) return;

        // Instantiate material so we don't affect shared materials
        Material mat = renderer.material;

        Color originalColor = mat.HasProperty("_MainColor")
            ? mat.GetColor("_MainColor")
            : mat.color;

        Color flashColor = Color.red;

        float flashDuration = 0.5f; // VERY fast

        mat
            .DOColor(flashColor, "_MainColor", flashDuration * 0.5f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                mat.DOColor(originalColor, "_MainColor", flashDuration * 0.5f)
                    .SetEase(Ease.InQuad);
            });
    }
}