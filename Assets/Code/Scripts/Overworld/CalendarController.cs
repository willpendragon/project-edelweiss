using UnityEngine;
using TMPro;
using ProjectEdelweiss.Utils;

public class CalendarController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _currentDayText;
    [SerializeField] private int _daysCounter = 1; // Starting value.
    private GameStatsManager _gameStatsManager;

    private void Start()
    {
        _gameStatsManager = GameObject.FindGameObjectWithTag(GameTags.GAME_STATS_MANAGER).GetComponent<GameStatsManager>();
        // Load the Current Day,
        _gameStatsManager.LoadCalendarData();
        _daysCounter = _gameStatsManager.currentDay;
        UpdateCurrentDayText();
    }

    // Update the Current Day Text
    private void UpdateCurrentDayText()
    {
        _currentDayText.text = $"Current Day: {_daysCounter}";
    }

    public void IncreaseDaysCounter(int days)
    {
        _daysCounter += days;
        UpdateCurrentDayText();
        GameManager.Instance.BuffManager.UpdateBuffs(days);
        // Save the Current Day.
        _gameStatsManager.SaveCalendarData(days);
        _gameStatsManager.SaveCharacterData();
    }
}
