using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Forecasts which Deities will appear in the upcoming battle and their spawn chances.
/// Handles two deity types:
/// - Overseer Deities: Present in regular battles but not directly attackable
/// - Capturable Deities: Unlocked via achievements, directly attackable (Battle with Deity)
/// </summary>
public class DeityForecastController : MonoBehaviour
{
    [SerializeField] private AchievementsManager _achievementsManager;
    [SerializeField] private DeitySpawner _deitySpawner;

    [Header("Settings")]
    [SerializeField] private float _overseerDeityChance = 0.43f; // (Roll 3-6 out of 0-7) / ~43%
    [SerializeField] private float _captureDeityChance = 0.5f; // 50% chance when achievement is unlocked

    /// <summary>
    /// Data class representing a single Deity forecast entry.
    /// </summary>
    [System.Serializable]
    public class DeityForecast
    {
        public Deity deity;
        public string deityName;
        public Sprite deityPortrait;
        public DeityType type;
        public float totalAppearanceChance;
        public float overseerChance;
        public float captureChance;

        public enum DeityType
        {
            OverseerOnly,
            CaptureOnly,
            Both
        }
    }

    /// <summary>
    /// Retrieves all possible Deity forecasts for an upcoming battle.
    /// </summary>
    public List<DeityForecast> GetDeityForecasts()
    {
        List<DeityForecast> forecasts = new List<DeityForecast>();

        // Add overseer deity forecasts (random spawn from DeitySpawner pool)
        AddOverseerDeityForecasts(ref forecasts);

        // Add capturable deity forecasts (from unlocked achievements)
        AddCapturableDeityForecasts(ref forecasts);

        // Calculate final probabilities
        CalculateFinalProbabilities(ref forecasts);

        return forecasts;
    }

    /// <summary>
    /// Adds forecasts for overseer deities from the DeitySpawner's spawnable pool.
    /// </summary>
    private void AddOverseerDeityForecasts(ref List<DeityForecast> forecasts)
    {
        if (_deitySpawner == null)
        {
            Debug.LogWarning("DeityForecastController: DeitySpawner not assigned.");
            return;
        }

        // Get all spawnable deities from the spawner
        var spawnableDeities = GetSpawnableDeities();

        if (spawnableDeities.Count == 0)
        {
            Debug.Log("DeityForecastController: No spawnable deities available.");
            return;
        }

        GameSaveData saveData = SaveStateManager.saveData;

        foreach (var deityGO in spawnableDeities)
        {
            if (deityGO == null)
                continue;

            Deity deity = deityGO.GetComponent<Deity>();
            if (deity == null)
                continue;

            Unit unitComponent = deityGO.GetComponent<Unit>();
            if (unitComponent == null)
                continue;

            string deityId = deity.Id;

            // Check if this deity is already killed
            if (saveData.killedDeities.ContainsKey(unitComponent.unitTemplate.unitName))
                continue;

            // Check if this deity is linked to any player
            if (saveData.unitsLinkedToDeities.ContainsValue(deityId))
                continue;

            // Check if this deity is captured but unassigned
            if (saveData.unassignedCapturedDeities.Contains(deityId))
                continue;

            // Get portrait from the unit's sprite renderer
            Sprite portrait = unitComponent.unitTemplate.unitPortrait != null ? unitComponent.unitTemplate.unitPortrait : null;

            // Create forecast entry for overseer deity
            DeityForecast forecast = new DeityForecast
            {
                deity = deity,
                deityName = unitComponent.unitTemplate.unitName,
                deityPortrait = portrait,
                type = DeityForecast.DeityType.OverseerOnly,
                overseerChance = _overseerDeityChance / spawnableDeities.Count,
                captureChance = 0f
            };

            forecasts.Add(forecast);
        }
    }

    /// <summary>
    /// Adds forecasts for deities unlocked through achievements.
    /// </summary>
    private void AddCapturableDeityForecasts(ref List<DeityForecast> forecasts)
    {
        if (_achievementsManager == null)
        {
            Debug.LogWarning("DeityForecastController: AchievementsManager not assigned.");
            return;
        }

        if (_achievementsManager.allAchievements == null || _achievementsManager.allAchievements.Count == 0)
        {
            Debug.Log("DeityForecastController: No achievements available.");
            return;
        }

        GameSaveData saveData = SaveStateManager.saveData;

        // Get all unlocked achievements
        var unlockedAchievements = _achievementsManager.allAchievements
            .Where(a => a != null && a.AchievementIsUnlocked())
            .ToList();

        foreach (var achievement in unlockedAchievements)
        {
            if (achievement.spawnableDeity == null)
                continue;

            Deity deity = achievement.spawnableDeity.GetComponent<Deity>();
            if (deity == null)
                continue;

            Unit unitComponent = achievement.spawnableDeity.GetComponent<Unit>();
            if (unitComponent == null)
                continue;

            string deityId = deity.Id;

            // Check if this deity is already killed
            if (saveData.killedDeities.ContainsKey(unitComponent.unitTemplate.unitName))
                continue;

            // Check if this deity is linked to any player
            if (saveData.unitsLinkedToDeities.ContainsValue(deityId))
                continue;

            // Check if this deity is captured but unassigned
            if (saveData.unassignedCapturedDeities.Contains(deityId))
                continue;

            // Get portrait from the unit's sprite renderer
            Sprite portrait = unitComponent.unitSprite != null ? unitComponent.unitSprite.sprite : null;

            // Check if deity already exists in list (might be added as both overseer and capturable)
            var existingForecast = forecasts.FirstOrDefault(f => f.deity == deity);

            if (existingForecast != null)
            {
                // Update type to "Both" and add capture chance
                existingForecast.type = DeityForecast.DeityType.Both;
                existingForecast.captureChance = _captureDeityChance;
            }
            else
            {
                // Create new forecast entry for capturable deity
                DeityForecast forecast = new DeityForecast
                {
                    deity = deity,
                    deityName = unitComponent.unitTemplate.unitName,
                    deityPortrait = portrait,
                    type = DeityForecast.DeityType.CaptureOnly,
                    overseerChance = 0f,
                    captureChance = _captureDeityChance
                };

                forecasts.Add(forecast);
            }
        }
    }

    /// <summary>
    /// Calculates final appearance probabilities based on deity type.
    /// </summary>
    private void CalculateFinalProbabilities(ref List<DeityForecast> forecasts)
    {
        foreach (var forecast in forecasts)
        {
            switch (forecast.type)
            {
                case DeityForecast.DeityType.OverseerOnly:
                    forecast.totalAppearanceChance = forecast.overseerChance;
                    break;

                case DeityForecast.DeityType.CaptureOnly:
                    forecast.totalAppearanceChance = forecast.captureChance;
                    break;

                case DeityForecast.DeityType.Both:
                    // Either overseer OR capture (mutually exclusive in a single battle)
                    forecast.totalAppearanceChance = forecast.overseerChance + forecast.captureChance;
                    break;

                default:
                    forecast.totalAppearanceChance = 0f;
                    break;
            }
        }
    }

    /// <summary>
    /// Gets the spawnable deities from DeitySpawner.
    /// This is a helper method that needs to access DeitySpawner's private list.
    /// Consider making DeitySpawner's list public or creating a public getter.
    /// </summary>
    private List<GameObject> GetSpawnableDeities()
    {
        // TODO: Modify DeitySpawner to expose its spawnableDeities list publicly
        // For now, this uses reflection as a workaround
        var fieldInfo = _deitySpawner.GetType().GetField("spawnableDeities",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (fieldInfo != null)
        {
            var deityList = fieldInfo.GetValue(_deitySpawner) as List<Deity>;
            if (deityList != null)
            {
                return deityList.Select(d => d.gameObject).ToList();
            }
        }

        Debug.LogWarning("DeityForecastController: Could not access DeitySpawner's spawnableDeities list.");
        return new List<GameObject>();
    }
}