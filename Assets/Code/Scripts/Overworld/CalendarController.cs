using UnityEngine;
using TMPro;
using ProjectEdelweiss.Utils;

public class CalendarController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _currentDayText;
    [SerializeField] TextMeshProUGUI _bloodMoonWarningText; // Hook your UI text here
    [SerializeField] private int _daysCounter = 1;
    [SerializeField] private FeedingHelper _feedingHelper;
    private GameStatsManager _gameStatsManager;
    private BloodMoonManager _bloodMoonManager;

    private void Start()
    {
        _gameStatsManager = GameObject.FindGameObjectWithTag(GameTags.GAME_STATS_MANAGER).GetComponent<GameStatsManager>();
        _bloodMoonManager = BloodMoonManager.Instance;
        
        _gameStatsManager.LoadCalendarData();
        _daysCounter = _gameStatsManager.currentDay;
        UpdateCurrentDayText();
        
        // Initialize blood moon display on startup
        if (_bloodMoonManager != null)
        {
            _bloodMoonManager.LoadBloodMoonState();
            UpdateBloodMoonDisplay();
        }
    }

    // Update the Current Day Text
    private void UpdateCurrentDayText()
    {
        _currentDayText.text = $"Current Day: {_daysCounter}";
    }

    private void UpdateBloodMoonDisplay()
    {
        if (_bloodMoonManager == null) return;

        if (_bloodMoonManager.IsBloodMoonActive)
        {
            if (_bloodMoonWarningText != null)
            {
                _bloodMoonWarningText.text = "BLOOD MOON";
                _bloodMoonWarningText.color = new Color(1f, 0.2f, 0.2f); // Red tint
                _bloodMoonWarningText.gameObject.SetActive(true);
            }
        }
        else
        {
            if (_bloodMoonWarningText != null)
            {
                _bloodMoonWarningText.gameObject.SetActive(false);
            }
        }
    }

    public void IncreaseDaysCounter(int days)
    {
        _daysCounter += days;
        UpdateCurrentDayText();
        
        // Update blood moon state before applying buffs
        _bloodMoonManager.UpdateBloodMoonState(_daysCounter);
        UpdateBloodMoonDisplay();
        
        GameManager.Instance.BuffManager.UpdateBuffs(days);
        // Reset the Units feeding stats after 1 day passes.
        _feedingHelper.ResetPartyFeedingStats();
        // Save the Current Day.
        _gameStatsManager.SaveCalendarData(days);
        _gameStatsManager.SaveCharacterData();
    }
}
