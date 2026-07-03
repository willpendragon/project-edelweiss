using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Simple display for a single Deity forecast entry.
/// Shows deity name, manifestation chance (overseer), and battle chance (capturable).
/// </summary>
public class DeityForecastEntryUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _deityNameText;
    [SerializeField] private TextMeshProUGUI _manifestationChanceText;
    [SerializeField] private TextMeshProUGUI _battleChanceText;

    /// <summary>
    /// Populates the entry with deity forecast data.
    /// </summary>
    public void Populate(DeityForecastController.DeityForecast forecast)
    {
        if (forecast == null)
        {
            Debug.LogWarning("DeityForecastEntryUI: Received null forecast.");
            return;
        }

        // Display deity name
        if (_deityNameText != null)
        {
            _deityNameText.text = forecast.deityName;
        }

        // Display manifestation chance (overseer only)
        if (_manifestationChanceText != null)
        {
            if (forecast.overseerChance > 0)
            {
                _manifestationChanceText.text = $"Manifestation: {forecast.overseerChance:P1}";
            }
            else
            {
                _manifestationChanceText.text = "Manifestation: —";
            }
        }

        // Display battle chance (capturable only)
        if (_battleChanceText != null)
        {
            if (forecast.captureChance > 0)
            {
                _battleChanceText.text = $"Battle Chance: {forecast.captureChance:P1}";
            }
            else
            {
                _battleChanceText.text = "Battle Chance: —";
            }
        }
    }

    /// <summary>
    /// Clears the UI.
    /// </summary>
    public void Clear()
    {
        if (_deityNameText != null)
            _deityNameText.text = "";

        if (_manifestationChanceText != null)
            _manifestationChanceText.text = "";

        if (_battleChanceText != null)
            _battleChanceText.text = "";
    }
}