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
}

[System.Serializable]
public class CharacterData
{
    public string unitId;
    public float unitHealthPoints;
    public float unitManaPoints;
    public float unitShieldPoints;
    public string name;
    public string description;
}

[System.Serializable]

public class ResourceData

{
    public float warFunds;
    public float experiencePointsReward;
    public int captureCrystalsCount;
}

