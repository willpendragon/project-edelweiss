using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class GameFlowController : MonoBehaviour
{
    private enum CompletionState
    {
        Standard,
        BossFightUnlocked,
        NextDomainUnlocked,
        DomainClear
    }

    [SerializeField] Transform bossLevelTowerSpawnpoint;
    [SerializeField] GameObject bossLevelTowerPrefab;
    [SerializeField] RectTransform endOfDemoPanel;
    [SerializeField] GraphicRaycaster overWorldMapCanvas;
    [SerializeField] private int _currentSessionHighestUnlockedLevel;
    [SerializeField] private int _bossFightRequirement;
    [SerializeField] private int _nextDomainUnlockRequirement;
    [SerializeField] private int _demoEndRequirement;
    [SerializeField] private CompletionState _completionState;
    [SerializeField] private Domain _currentDomain;
    [SerializeField] private OverworldMapGenerator _overworldMapGenerator;
    public List<Domain> domains = new List<Domain>();
    void Awake()
    {
        LoadProgress();
        SetDomainData();
        SetCompletionState(_currentSessionHighestUnlockedLevel);
        GenerateLevel();
        UnlockEvents();
    }

    private void GenerateLevel()
    {
        if (_overworldMapGenerator == null)
            return;
        _overworldMapGenerator.GenerateLevel(_currentDomain);
    }

    private void LoadProgress()
    {
        if (SaveStateManager.saveData.highestUnlockedLevel <= 0)
            return;
        _currentSessionHighestUnlockedLevel = SaveStateManager.saveData.highestUnlockedLevel;
    }

    private void SetDomainData()
    {
        _currentDomain = domains[_currentSessionHighestUnlockedLevel];
        _bossFightRequirement = _currentDomain.bossFightRequirement;
        _nextDomainUnlockRequirement = _currentDomain.nextDomainRequirement;
        _demoEndRequirement = _currentDomain.clearRequirement;
    }

    private void SetCompletionState(int currentSessionHighestUnlockedLevel)
    {
        if (_currentSessionHighestUnlockedLevel == _bossFightRequirement)
        {
            _completionState = CompletionState.BossFightUnlocked;
        }
        else if (_currentSessionHighestUnlockedLevel == _demoEndRequirement)
        {
            _completionState = CompletionState.BossFightUnlocked;
        }
        else if (_currentSessionHighestUnlockedLevel == _nextDomainUnlockRequirement)
        {
            _completionState = CompletionState.DomainClear;
        }
    }

    private void UnlockEvents()
    {
        switch (_completionState)
        {
            case CompletionState.BossFightUnlocked:
                UnlockBossFight();
                break;
            case CompletionState.NextDomainUnlocked:
                UnlockNewDomain();
                break;
            case CompletionState.DomainClear:
                UnlockDemoEnd();
                break;
        }
    }

    private void UnlockBossFight()
    {
        // GameObject newBossLevelTower = Instantiate(bossLevelTowerPrefab, bossLevelTowerSpawnpoint);
        // Debug.Log("Boss Jacob's Ladder appears");
    }
    private void UnlockNewDomain()
    {
        int nextDomainIndex = _currentSessionHighestUnlockedLevel += 1;
        _currentDomain = domains[nextDomainIndex];
        // Presentation logic to display level unlock.
    }

    private void UnlockDemoEnd()
    {
        // Hard-coded logic for demo end.
        overWorldMapCanvas.enabled = true;
        endOfDemoPanel.localScale = Vector3.one;
    }
}