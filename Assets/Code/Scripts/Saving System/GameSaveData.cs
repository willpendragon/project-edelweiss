using System;
using System.Collections.Generic;

[System.Serializable]
public class GameSaveData
{
    public int enemiesKilled;
    public int highestUnlockedLevel;
    public int timesSingleTargetSpellWasUsed;


    public List<CharacterData> characterData = new List<CharacterData>();
    public ResourceData resourceData = new ResourceData();

    public Dictionary<string, string> unitsLinkedToDeities = new Dictionary<string, string>();
    public List<ConversationData> unlockedConversations = new List<ConversationData>();

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
}

[System.Serializable]

public class ResourceData

{
    public float warFunds;
    public float experiencePointsReward;
    public int captureCrystalsCount;
}

[System.Serializable]
public class ConversationData
{
    public string conversationID;
    public bool isUnlocked;
    public bool isRead;

    // Constructor with ID and unlocked status
    public ConversationData(string id, bool unlocked, bool read)
    {
        conversationID = id;
        isUnlocked = unlocked;
        isRead = read;
    }
}

