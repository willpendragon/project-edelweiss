using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
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
        if (BattleTypeController.Instance.currentBattleType == BattleTypeController.BattleType.RegularBattle)
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

            // Hides the Deity Health Bar when not necessary.
            if (BattleManager.Instance.deity == null)
                return;
            var healthBar = BattleManager.Instance.deity.GetComponentInChildren<DeityHealthBar>();
            healthBar.HideHealthBar();
        }
        else if (BattleTypeController.Instance.currentBattleType == BattleTypeController.BattleType.BattleWithDeity)
        {
            if (deityHealthBarInstance != null)
            {
                PopulateDeityHealthBar();
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
        // Load the Killed Deity Dictionary
        // If there is a match between a Deity in the Spawnable Deity array
        // And a Deity in the Killed Deity Dictionary
        // Delete that Spawnable Deity from the Dictionary

        for (int i = spawnableDeities.Count - 1; i >= 0; i--)
        {
            var deity = spawnableDeities[i];
            string deityName = deity.gameObject.GetComponent<Unit>().unitTemplate.unitName;

            bool deityIsKilled =
                _killedDeityDictionary.ContainsKey(deityName) &&
                _killedDeityDictionary[deityName];

            if (deityIsKilled == true)
            {
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
        Debug.Log("Rolling which Deity will appear");
        int deityIndex = localRandom.Next(0, spawnableDeities.Count); // Use System.Random for Deity selection
        Debug.Log($"Deity Index: {deityIndex} - {spawnableDeities[deityIndex].name}");

        GameObject spawningDeity = spawnableDeities[deityIndex].gameObject;
        SpawnDeity(spawningDeity);
    }
    public void SpawnDeity(GameObject spawningDeity)
    {
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
        Debug.Log($"Unlocked {unlockedDeity.GetComponent<Unit>().unitTemplate.unitName}");

        string deityName = unlockedDeity.GetComponent<Unit>().unitTemplate.unitName;

        if (DeityIsKilled(deityName))
        {
            return;
        }

        int unlockedDeityStartingTileXCoordinate = 5;
        int unlockedDeityStartingTileYCoordinate = 5;

        unlockedDeity.GetComponent<Unit>().startingXCoordinate = unlockedDeityStartingTileXCoordinate;
        unlockedDeity.GetComponent<Unit>().startingYCoordinate = unlockedDeityStartingTileYCoordinate;
        GameObject unboundDeity = Instantiate(unlockedDeity, deitySpawnPosition.position, Quaternion.identity);
        Debug.Log($"Instantiate Unbound Deity GameObject at {deitySpawnPosition}");

        if (unboundDeity != null)
        {
            Debug.Log("Start of Summon Deity on Battlefield");
            int deityTilePositionX = 5;
            int deityTilePositionY = 5;
            //unboundDeity.GetComponent<Unit>().MoveUnit(deityTilePositionX, deityTilePositionY, false);
            TileController firstDeitySpawningTile = GridManager.Instance.GetTileControllerInstance(deityTilePositionX, deityTilePositionY);

            unboundDeity.GetComponent<Unit>().ownedTile = firstDeitySpawningTile;
            _deityObeliskInstance = Instantiate(deityObelisk, deityObeliskSpawningPoint.transform);

            GridMovementController gridMovementController = GameObject.FindGameObjectWithTag("GridMovementController").GetComponent<GridMovementController>();
            firstDeitySpawningTile.currentSingleTileCondition = SingleTileCondition.occupiedByDeity;
            firstDeitySpawningTile.detectedUnit = unboundDeity;
            currentUnboundDeity = unboundDeity.GetComponent<Deity>();
            _enemyTurnManager.deity = unboundDeity;
            Debug.Log("Deity occupies Tile");
        }

        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            Destroy(enemy);
        }
        unboundDeity.gameObject.tag = "Enemy";
    }
    public bool DeityIsKilled(string deityName)
    {
        if (_killedDeityDictionary.TryGetValue(deityName, out bool isKilled) && isKilled)
        {
            Debug.Log($"{deityName} has been killed, Player can't fight it");
            return true;
        }
        else
        {
            return false;
        }
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

        Color originalColor = mat.HasProperty("_BaseColor")
            ? mat.GetColor("_BaseColor")
            : mat.color;

        Color flashColor = Color.red;

        float flashDuration = 0.5f; // VERY fast

        mat
            .DOColor(flashColor, "_BaseColor", flashDuration * 0.5f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                mat.DOColor(originalColor, "_BaseColor", flashDuration * 0.5f)
                   .SetEase(Ease.InQuad);
            });
    }
}