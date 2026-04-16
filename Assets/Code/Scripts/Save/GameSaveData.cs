using System.Collections.Generic;
using static ConversationData;

[System.Serializable]
public class GameSaveData
{
    public int enemiesKilled;
    public int highestUnlockedLevel;
    public int currentNodeId; // Add this line here
    public List<int> clearedNodesId = new List<int>();
    public int timesSingleTargetSpellWasUsed;
    public int convoIndex;
    public int lastConvoNumber;

    public CalendarData calendarData = new CalendarData();
    public ResourceData resourceData = new ResourceData();
    public GameFlowData gameFlowData = new GameFlowData();

    public List<CharacterData> characterData = new List<CharacterData>();
    public Dictionary<string, string> unitsLinkedToDeities = new Dictionary<string, string>();
    public List<ConversationData> unlockedConversations = new List<ConversationData>();
    public List<IngredientSaveEntry> savedInventory = new List<IngredientSaveEntry>();
    public List<BakedItemsData> bakedItems = new List<BakedItemsData>();
    public Dictionary<string, bool> killedDeities = new Dictionary<string, bool>();
    public List<EatenPastryData> eatenPastriesHistory = new List<EatenPastryData>();
    public int runSeed = 0; // 0 means a new run hasn't generated a seed yet
}

[System.Serializable]

public class CalendarData
{
    public int currentDay;
}

[System.Serializable]

public class GameFlowData
{
    public bool secretLevelUnlocked; // Demo Logic only
}

[System.Serializable]
public class SavedBuffEntry
{
    public FoodBuff.FoodBuffType type;
    public float appliedValue;
    public int remainingDuration;
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

    public List<SavedBuffEntry> activeBuffs = new List<SavedBuffEntry>();
}

[System.Serializable]

public class ResourceData
{
    public float warFunds;
    public float experiencePointsReward;
    public int captureCrystalsCount;
    public int puzzleLevelKeys;
    public bool hasMinibossKey;
    public bool hasBossKey;
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
    public int conversationNumber;

    // Constructor with ID and unlocked status.
    public ConversationData(string id, bool unlocked, bool read, int number)
    {
        conversationID = id;
        isUnlocked = unlocked;
        isRead = read;
        conversationNumber = number;
    }
}

[System.Serializable]
public class EatenPastryData
{
    public string unitId; // To identify which character ate it
    public List<string> foodNames = new List<string>();

    public EatenPastryData(string id, List<string> names)
    {
        this.unitId = id;
        this.foodNames = names;
    }
}
