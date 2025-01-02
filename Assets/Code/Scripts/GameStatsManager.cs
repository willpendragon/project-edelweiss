using UnityEngine;

public class GameStatsManager : MonoBehaviour

{
    public int enemiesKilled;
    public float warFunds;
    public int timesSingleTargetSpellWasUsed;
    public int captureCrystalsCount;
    public int unlockedPuzzleKeys;

    private CharacterData characterData;

    public void Awake()
    {
        LoadWarFunds();
        LoadEnemiesKilled();
        LoadUsedSingleTargetSpells();
        LoadCaptureCrystalsCount();
        LoadUnlockedKeys();
    }

    public void Start()
    {
        LoadCharacterData();
    }

    public void SaveCharacterData()
    {
        GameObject[] playerUnits = GameObject.FindGameObjectWithTag("BattleManager").GetComponentInChildren<TurnController>().playerUnitsOnBattlefield;
        GameSaveData characterSaveData = SaveStateManager.saveData;

        foreach (var playerUnit in playerUnits)
        {
            Unit unitComponent = playerUnit.GetComponent<Unit>();
            CharacterData existingCharacterData = characterSaveData.characterData.Find(character => character.unitId == unitComponent.Id);

            if (existingCharacterData != null)
            {
                // Update existing character data
                existingCharacterData.unitHealthPoints = unitComponent.unitHealthPoints;
                existingCharacterData.unitSavedManaPoints = unitComponent.unitManaPoints;
                existingCharacterData.unitShieldPoints = unitComponent.unitShieldPoints;

                existingCharacterData.unitLifeCondition = unitComponent.currentUnitLifeCondition;

                existingCharacterData.unitAttackPower = unitComponent.unitAttackPower;
                existingCharacterData.unitMagicPower = unitComponent.unitMagicPower;

                // Update other stats as necessary
            }
            else if (existingCharacterData == null)
            {
                // Add new character data
                CharacterData newCharacterData = new CharacterData()
                {
                    unitId = unitComponent.Id,
                    unitHealthPoints = unitComponent.unitHealthPoints,
                    unitSavedManaPoints = unitComponent.unitManaPoints,
                    unitShieldPoints = unitComponent.unitShieldPoints,

                    unitLifeCondition = unitComponent.currentUnitLifeCondition,

                    unitAttackPower = unitComponent.unitAttackPower,
                    unitMagicPower = unitComponent.unitMagicPower,


                };
                characterSaveData.characterData.Add(newCharacterData);
            }
        }

        SaveStateManager.SaveGame(characterSaveData);
    }
    public void LoadCharacterData()
    {
        Debug.Log("Loading Player Character's Data");
        GameObject[] playerUnits = null;
        if (TurnController.Instance != null)
        {
            playerUnits = TurnController.Instance.playerUnitsOnBattlefield;
        }
        if (playerUnits != null)
        {
            GameSaveData characterSaveData = SaveStateManager.saveData;
            foreach (var playerUnit in playerUnits)
            {
                Unit unitComponent = playerUnit.GetComponent<Unit>();
                CharacterData loadedCharacterData = characterSaveData.characterData.Find(character => character.unitId == unitComponent.Id);
                if (loadedCharacterData != null)
                {
                    unitComponent.unitHealthPoints = loadedCharacterData.unitHealthPoints;
                    unitComponent.unitManaPoints = loadedCharacterData.unitSavedManaPoints;
                    unitComponent.unitShieldPoints = loadedCharacterData.unitShieldPoints;

                    unitComponent.currentUnitLifeCondition = loadedCharacterData.unitLifeCondition;

                    unitComponent.unitAttackPower = loadedCharacterData.unitAttackPower;
                    unitComponent.unitMagicPower = loadedCharacterData.unitMagicPower;

                    // Set other stats as necessary
                    Debug.Log("Restoring Player Units HP and Mana");
                }
            }
        }
    }
    public void LoadWarFunds()
    {
        GameSaveData resourceSaveData = SaveStateManager.saveData;
        if (resourceSaveData != null && resourceSaveData.resourceData != null)
        {
            warFunds = resourceSaveData.resourceData.warFunds;
            Debug.Log($"Loaded War Funds: {warFunds}");
        }
    }
    public void SaveWarFunds(float newWarFunds)
    {
        GameSaveData gameSaveData = SaveStateManager.saveData;
        if (gameSaveData.resourceData != null)
        {
            gameSaveData.resourceData.warFunds += newWarFunds;
            SaveStateManager.SaveGame(gameSaveData);
            Debug.Log($"Saved War Funds: {newWarFunds}");
        }
    }

    public void SaveSpentWarFunds(float spentWarFunds)
    {
        GameSaveData gameSaveData = SaveStateManager.saveData;
        if (gameSaveData.resourceData != null)
        {
            gameSaveData.resourceData.warFunds -= spentWarFunds;
            SaveStateManager.SaveGame(gameSaveData);
            Debug.Log($"Spent War Funds: {spentWarFunds}");
        }
    }
    public void LoadUnlockedKeys()
    {
        GameSaveData gameSaveData = SaveStateManager.saveData;
        if (gameSaveData != null)
        {
            unlockedPuzzleKeys = gameSaveData.resourceData.puzzleLevelKeys;
            Debug.Log("Loading the number of unlocked Keys");
        }
    }

    public void SaveUnlockedKeys(int unlockedKeys)
    {
        GameSaveData gameSaveData = SaveStateManager.saveData;
        if (gameSaveData.resourceData != null)
        {
            gameSaveData.resourceData.puzzleLevelKeys += unlockedKeys;
            SaveStateManager.SaveGame(gameSaveData);
            Debug.Log($"Saved Keys: {unlockedKeys}");
        }
    }
    public void LoadEnemiesKilled()
    {
        GameSaveData gameSaveData = SaveStateManager.saveData;
        if (gameSaveData != null)
        {
            enemiesKilled = gameSaveData.enemiesKilled;
            Debug.Log("Loading the number of killed Enemies");
        }
    }

    public void LoadCaptureCrystalsCount()
    {
        GameSaveData resourceSaveData = SaveStateManager.saveData;
        if (resourceSaveData != null && resourceSaveData.resourceData != null)
        {
            captureCrystalsCount = resourceSaveData.resourceData.captureCrystalsCount;
            Debug.Log($"Loaded Capture Crystals: {captureCrystalsCount}");
        }
    }

    public void SaveCaptureCrystalsCount()
    {
        GameSaveData gameSaveData = SaveStateManager.saveData;
        if (gameSaveData.resourceData != null)
        {
            gameSaveData.resourceData.captureCrystalsCount = captureCrystalsCount;
            SaveStateManager.SaveGame(gameSaveData);
            Debug.Log($"Saved Capture Crystals: {captureCrystalsCount}");
        }
    }

    public void SaveEnemiesKilled()
    {
        Debug.Log("Increasing Enemies Killed");

        // Prepare the save data
        GameSaveData saveData = SaveStateManager.saveData;
        saveData.enemiesKilled = enemiesKilled;

        SaveStateManager.SaveGame(saveData);
    }

    public void LoadUsedSingleTargetSpells()
    {
        GameSaveData gameSaveData = SaveStateManager.saveData;
        if (gameSaveData != null)
        {
            timesSingleTargetSpellWasUsed = gameSaveData.timesSingleTargetSpellWasUsed;
            Debug.Log("Loading the number of killed Enemies");
        }
    }

    public void SaveUsedSingleTargetSpells()
    {
        Debug.Log("Increasing Used Single Target Spells statistics");

        // Prepare the save data
        GameSaveData saveData = SaveStateManager.saveData;
        saveData.timesSingleTargetSpellWasUsed = timesSingleTargetSpellWasUsed;

        SaveStateManager.SaveGame(saveData);
    }
}
