using System;
using TMPro;
using UnityEngine;

public class OverworldUIManager : MonoBehaviour
{
    public static OverworldUIManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI _keyCounterText;
    [SerializeField] private TextMeshProUGUI _miniBossKeyCounterText;
    [SerializeField] private TextMeshProUGUI _bossKeyCounterText;
    [SerializeField] private EventsUIManager _eventsUIManager;
    [SerializeField] private TextMeshProUGUI _completedNodesCounter;

    [SerializeField] private OverworldMapGenerator _overWorldMapGenerator;
    [SerializeField] private GameStatsManager _gameStatsManager;

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
        UpdateCompletedNodesCounter();
    }

    public EventsUIManager EventsUIManager => _eventsUIManager;

    public void UpdateKeyCounterText(string message)
    {
        _keyCounterText.text = message;
    }

    public void UpdateMiniBossKeyCounterText(string message)
    {
        _miniBossKeyCounterText.text = message;
    }

    public void UpdateBossKeyCounterText(string message)
    {
        _bossKeyCounterText.text = message;
    }

    public void UpdateCompletedNodesCounter()
    {
        // Quick solution, refactor later.
        int currentUnlockedLevel = SaveStateManager.saveData.highestUnlockedLevel;
        int clearRequirement = _overWorldMapGenerator.CurrentDomain.clearRequirement;
        _completedNodesCounter.text = $"Completed Nodes {currentUnlockedLevel}/{clearRequirement}";
    }
}