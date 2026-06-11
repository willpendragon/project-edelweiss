using UnityEngine;
using TMPro;

public class BloodMoonManager : MonoBehaviour
{
    public static BloodMoonManager Instance { get; private set; }

    [SerializeField] private BloodMoonConfig _bloodMoonConfig;
    [SerializeField] private TextMeshProUGUI _bloodMoonUIText;
    private bool _isBloodMoonActive;
    private int _bloodMoonStartDay;

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
        int currentDay = saveData.calendarData.currentDay;

        // Check if blood moon should be active based on cycle
        CheckBloodMoonCycle(currentDay);
    }

    public void UpdateBloodMoonState(int currentDay)
    {
        if (_bloodMoonConfig == null) return;

        // Recalculate based on current day
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

    private void UpdateBloodMoonUI()
    {
        if (_bloodMoonUIText == null)
            return;

        if (_isBloodMoonActive)
        {
            _bloodMoonUIText.text = "BLOOD MOON IS ACTIVE";
            _bloodMoonUIText.color = new Color(1f, 0.2f, 0.2f); // Red tint
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