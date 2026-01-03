using JetBrains.Annotations;
using System;
using System.Collections.Generic;

[System.Serializable]
public class GameSaveData
{
    public int enemiesKilled;
    public int highestUnlockedLevel;
    public int timesSingleTargetSpellWasUsed;

    public CalendarData calendarData = new CalendarData();
    public ResourceData resourceData = new ResourceData();

    public List<CharacterData> characterData = new List<CharacterData>();
    public Dictionary<string, string> unitsLinkedToDeities = new Dictionary<string, string>();
    public List<ConversationData> unlockedConversations = new List<ConversationData>();
    public List<IngredientSaveEntry> savedInventory = new List<IngredientSaveEntry>();
    public List<BakedItemsData> bakedItems = new List<BakedItemsData>();
    public Dictionary<string, bool> killedDeities = new Dictionary<string, bool>();
}

[System.Serializable]

public class CalendarData
{
    public int currentDay;
}

[System.Serializable]
public class CharacterData
{
    public string unitId;
    public float unitHealthPoints;
    public float unitSavedManaPoints;
    public float unitShieldPoints;
    public string name;
    public string description;
    public Unit.UnitLifeCondition unitLifeCondition;

    public float unitAttackPower;
    public float unitMagicPower;
    public int unitFaithPoints;
    public int unitOccupiedFoodSlots;
}

[System.Serializable]

public class ResourceData
{
    public float warFunds;
    public float experiencePointsReward;
    public int captureCrystalsCount;
    public int puzzleLevelKeys;
}

[System.Serializable]
public class IngredientSaveEntry
{
    public string ingredientName;
    public int quantity;
}

[System.Serializable]
public class BakedItemsData
{
    public string pastryName;
    public int quantity;
}

[System.Serializable]
public class ConversationData
{
    public string conversationID;
    public bool isUnlocked;
    public bool isRead;

    // Constructor with ID and unlocked status.
    public ConversationData(string id, bool unlocked, bool read)
    {
        conversationID = id;
        isUnlocked = unlocked;
        isRead = read;
    }
}