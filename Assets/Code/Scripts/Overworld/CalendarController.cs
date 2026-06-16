using UnityEngine;
using TMPro;
using ProjectEdelweiss.Utils;

public class CalendarController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _currentDayText;
    [SerializeField] TextMeshProUGUI _bloodMoonWarningText;
    [SerializeField] private int _daysCounter = 1;
    [SerializeField] private FeedingHelper _feedingHelper;
    [SerializeField] private CanvasGroup _calendarCanvasGroup;
    private GameStatsManager _gameStatsManager;
    private BloodMoonManager _bloodMoonManager;

    private void Start()
    {
        _gameStatsManager = GameObject.FindGameObjectWithTag(GameTags.GAME_STATS_MANAGER)
            .GetComponent<GameStatsManager>();
        _bloodMoonManager = BloodMoonManager.Instance;

        _gameStatsManager.LoadCalendarData();
        _daysCounter = _gameStatsManager.currentDay;
        UpdateCurrentDayText();

        // Show blood moon info at game start
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

        string statusMessage = _bloodMoonManager.GetBloodMoonStatusMessage();

        if (_bloodMoonWarningText != null && !string.IsNullOrEmpty(statusMessage))
        {
            _bloodMoonWarningText.text = statusMessage;
            _bloodMoonWarningText.color = _bloodMoonManager.IsBloodMoonActive
                ? new Color(1f, 0.2f, 0.2f) // Red for active
                : new Color(1f, 0.7f, 0.2f); // Orange for warning
            _bloodMoonWarningText.gameObject.SetActive(true);
        }
        else if (_bloodMoonWarningText != null)
        {
            _bloodMoonWarningText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Call this when the player clicks on a battle node to hide the blood moon info
    /// </summary>
    public void HideBloodMoonInfo()
    {
        if (_bloodMoonWarningText != null)
        {
            _bloodMoonWarningText.gameObject.SetActive(false);
        }
    }

    public void IncreaseDaysCounter(int days)
    {
        _daysCounter += days;
        UpdateCurrentDayText();

        // Update blood moon state after returning from battle
        _bloodMoonManager.UpdateBloodMoonState(_daysCounter);

        // Show blood moon info when returning from battle
        UpdateBloodMoonDisplay();

        GameManager.Instance.BuffManager.UpdateBuffs(days);
        // Reset the Units feeding stats after 1 day passes.
        _feedingHelper.ResetPartyFeedingStats();
        // Save the Current Day.
        _gameStatsManager.SaveCalendarData(days);
        _gameStatsManager.SaveCharacterData();
    }

    public void HideCalendarInfo()
    {
        _calendarCanvasGroup.alpha = 0;
        _bloodMoonWarningText.gameObject.SetActive(false);
    }
}