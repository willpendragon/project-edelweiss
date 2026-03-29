using UnityEngine;

public class NodesUnlockManager : MonoBehaviour
{
    [SerializeField] private GameObject _mapNode; // The GO template for nodes.
    [SerializeField] private EnemyPartyData _enemies;
    [SerializeField] private MapData _mapData;
    [SerializeField] private Transform _nodeSpawnPoint;
    [SerializeField] private GameStatsManager _gameStatsManager;
    [SerializeField] private EventsUIManager _eventsUIManager;
    [SerializeField] NotificationConfig _secretNodeUnlockNotificationConfig;

    public MapData MapData => _mapData;

    // Flow related bools

    void Start()
    {
        GameManager.Instance.AddNodesUnlockManager(this);
        _gameStatsManager.LoadGameFlowData();
        UnlockSecretNodes();
    }

    // Add a dialogue or similar that triggers when the level is unlocked for the first time

    private void UnlockSecretNodes()
    {
        if (IsLevelKeyAvailable() == false)
            return;
        DisplayNodeUnlockedMessage();
        //SpendKeyResource();
        GenerateNode();
        UpdateKeyNumberOnUI();
    }

    private void DisplayNodeUnlockedMessage()
    {
        // This prevents the UI message from showing if the player already unlocked the secret level.
        if (_gameStatsManager.SecretLevelUnlocked == true) // Should instead retrieve a list of unlocked levels.
            return;
        OverworldUIManager.Instance.EventsUIManager.AddNotification(_secretNodeUnlockNotificationConfig, "Secret Level Unlocked: Similde's Glacial Lair", "Secret Level, encounter with Deity Similde");
        _gameStatsManager.SaveGameFlowData(true);
    }

    private void UpdateKeyNumberOnUI()
    {
        GameSaveData gameSaveData = SaveStateManager.saveData;
        int keyCount = gameSaveData.resourceData.puzzleLevelKeys;
        string message = $"Key Count: {keyCount}"; // Also add the reference to the icon in the Font Asset.
        OverworldUIManager.Instance.UpdateKeyCounterText(keyCount.ToString());
    }

    private bool IsLevelKeyAvailable()
    {
        GameSaveData gameSaveData = SaveStateManager.saveData;

        if (gameSaveData == null)
            return false;

        int unlockedLevelsKeys = gameSaveData.resourceData.puzzleLevelKeys;

        if (unlockedLevelsKeys >= 1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    private void GenerateNode()
    {
        // Notify Node Appearing
        // Spawn the Node GameObject
        GameObject unlockedNode = Instantiate(_mapNode, _nodeSpawnPoint);
        // Add properties to it using Scriptable Object (hard-coded is OK for demo)
        var nodeController = unlockedNode.GetComponent<MapNodeController>();
        nodeController.currentLockStatus = MapNodeController.LockStatus.levelUnlocked;
        var enemySelection = unlockedNode.GetComponent<EnemySelection>();
        enemySelection.enemyParty = _enemies;
        enemySelection.mapData = _mapData;
    }
    public void SpendKeyResource()
    {
        GameSaveData gameSaveData = SaveStateManager.saveData;
        if (gameSaveData.gameFlowData.secretLevelUnlocked == true)
        {
            gameSaveData.resourceData.puzzleLevelKeys--;
            SaveStateManager.SaveGame(gameSaveData);
        }
    }
}