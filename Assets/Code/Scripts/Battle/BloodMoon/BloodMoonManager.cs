using UnityEngine;
using TMPro;

public class BloodMoonManager : MonoBehaviour
{
    public static BloodMoonManager Instance { get; private set; }

    [SerializeField] private BloodMoonConfig _bloodMoonConfig;
    [SerializeField] private TextMeshProUGUI _bloodMoonUIText;
    private bool _isBloodMoonActive;
    private int _bloodMoonStartDay;
    private int _currentDay;

    public bool IsBloodMoonActive => _isBloodMoonActive;
    public float EnemyAttackMultiplier => _isBloodMoonActive ? _bloodMoonConfig.EnemyAttackPowerMultiplier : 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Load blood moon state from save data
        LoadBloodMoonState();
        UpdateBloodMoonUI();
    }

    public void LoadBloodMoonState()
    {
        if (_bloodMoonConfig == null)
        {
            Debug.LogError("BloodMoonConfig is not assigned!");
            return;
        }

        GameSaveData saveData = SaveStateManager.saveData;
        _currentDay = saveData.calendarData.currentDay;

        // Check if blood moon should be active based on cycle
        CheckBloodMoonCycle(_currentDay);
    }

    public void UpdateBloodMoonState(int currentDay)
    {
        if (_bloodMoonConfig == null) return;

        _currentDay = currentDay;
        CheckBloodMoonCycle(currentDay);
        UpdateBloodMoonUI();
    }

    private void CheckBloodMoonCycle(int currentDay)
    {
        if (_bloodMoonConfig == null || currentDay < 1) return;

        GameSaveData saveData = SaveStateManager.saveData;
        int cycleDays = _bloodMoonConfig.DaysBeforeBloodMoon + _bloodMoonConfig.BloodMoonDuration;
        
        // Where are we in the current cycle?
        int dayInCycle = (currentDay - 1) % cycleDays;
        
        // Blood moon triggers after the interval and lasts for the duration
        int bloodMoonStartInCycle = _bloodMoonConfig.DaysBeforeBloodMoon;
        int bloodMoonEndInCycle = _bloodMoonConfig.DaysBeforeBloodMoon + _bloodMoonConfig.BloodMoonDuration - 1;

        _isBloodMoonActive = dayInCycle >= bloodMoonStartInCycle && dayInCycle <= bloodMoonEndInCycle;
        _bloodMoonStartDay = currentDay - dayInCycle + bloodMoonStartInCycle;

        // Update save data
        saveData.bloodMoonData.isActive = _isBloodMoonActive;
        if (_isBloodMoonActive)
        {
            saveData.bloodMoonData.bloodMoonStartDay = _bloodMoonStartDay;
        }

        Debug.Log($"[Blood Moon] Day {currentDay} (cycle day {dayInCycle + 1}) - Blood Moon Active: {_isBloodMoonActive}");
    }

    public string GetBloodMoonStatusMessage()
    {
        if (_bloodMoonConfig == null) return "";

        int cycleDays = _bloodMoonConfig.DaysBeforeBloodMoon + _bloodMoonConfig.BloodMoonDuration;
        int dayInCycle = (_currentDay - 1) % cycleDays;
        int bloodMoonStartInCycle = _bloodMoonConfig.DaysBeforeBloodMoon;
        int bloodMoonEndInCycle = _bloodMoonConfig.DaysBeforeBloodMoon + _bloodMoonConfig.BloodMoonDuration - 1;

        if (_isBloodMoonActive)
        {
            // Blood moon is active - show days remaining
            int daysRemainingInBloodMoon = bloodMoonEndInCycle - dayInCycle;
            if (daysRemainingInBloodMoon == 0)
            {
                return "Alert: No Blood Moon in next battle";
            }
            else
            {
                return $"Alert: Blood Moon in next battle. -{daysRemainingInBloodMoon} day(s) to Blood Moon end";
            }
        }
        else
        {
            // Blood moon is NOT active - show days until it starts
            int daysUntilBloodMoon = bloodMoonStartInCycle - dayInCycle;
            
            if (daysUntilBloodMoon == 1)
            {
                return "Alert: Blood Moon will start in next battle";
            }
            else
            {
                return $"{daysUntilBloodMoon} day(s) to Blood Moon";
            }
        }
    }

    private void UpdateBloodMoonUI()
    {
        if (_bloodMoonUIText == null)
            return;

        string statusMessage = GetBloodMoonStatusMessage();
        
        if (!string.IsNullOrEmpty(statusMessage))
        {
            _bloodMoonUIText.text = statusMessage;
            _bloodMoonUIText.color = _isBloodMoonActive ? new Color(1f, 0.2f, 0.2f) : new Color(1f, 0.7f, 0.2f); // Red for active, orange for warning
            _bloodMoonUIText.gameObject.SetActive(true);
        }
        else
        {
            _bloodMoonUIText.gameObject.SetActive(false);
        }
    }

    public bool IsBloodMoonHappeningToday(int currentDay)
    {
        return _isBloodMoonActive;
    }
}