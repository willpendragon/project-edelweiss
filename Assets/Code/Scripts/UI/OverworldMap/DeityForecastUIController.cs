using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages the Deity Forecast UI panel on the overworld map.
/// Displays a simple list of deities with their manifestation and battle chances.
/// </summary>
public class DeityForecastUIController : MonoBehaviour
{
    [SerializeField] private DeityForecastController _forecastController;
    [SerializeField] private Transform _forecastContainer;
    [SerializeField] private GameObject _forecastEntryPrefab;

    private List<DeityForecastEntryUI> _activeEntries = new List<DeityForecastEntryUI>();

    /// <summary>
    /// Displays all deity forecasts in the UI.
    /// </summary>
    /// 
    //private void Start()
    //{
    //    if (_forecastController == null)
    //    {
    //        Debug.LogError("DeityForecastUIController: DeityForecastController not assigned.");
    //        return;
    //    }
    //    DisplayDeityForecasts(_forecastContainer);
    //}

    public void DisplayDeityForecasts(Transform uiContainer)
    {
        _forecastContainer = uiContainer;
        if (_forecastController == null)
        {
            Debug.LogError("DeityForecastUIController: DeityForecastController not assigned.");
            return;
        }

        // Clear existing entries
        ClearForecasts();

        // Get forecasts from controller
        List<DeityForecastController.DeityForecast> forecasts = _forecastController.GetDeityForecasts();

        if (forecasts.Count == 0)
        {
            Debug.Log("DeityForecastUIController: No deity forecasts available.");
            return;
        }

        // Populate UI with forecasts
        foreach (var forecast in forecasts)
        {
            if (_forecastEntryPrefab == null)
            {
                Debug.LogError("DeityForecastUIController: Forecast entry prefab not assigned.");
                break;
            }

            GameObject entryGO = Instantiate(_forecastEntryPrefab, _forecastContainer);
            DeityForecastEntryUI entryUI = entryGO.GetComponent<DeityForecastEntryUI>();

            if (entryUI == null)
            {
                Debug.LogError("DeityForecastUIController: Forecast entry prefab missing DeityForecastEntryUI component.");
                Destroy(entryGO);
                continue;
            }

            entryUI.Populate(forecast);
            _activeEntries.Add(entryUI);
        }
    }

    /// <summary>
    /// Clears all forecast entries from the UI.
    /// </summary>
    public void ClearForecasts()
    {
        foreach (var entry in _activeEntries)
        {
            if (entry != null)
            {
                entry.Clear();
                Destroy(entry.gameObject);
            }
        }

        _activeEntries.Clear();
    }

    /// <summary>
    /// Refreshes the forecast display.
    /// </summary>
    public void RefreshForecasts()
    {
        DisplayDeityForecasts(_forecastContainer);
    }
}
