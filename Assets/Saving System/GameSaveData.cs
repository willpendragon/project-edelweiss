using System;
using System.Collections.Generic;

[System.Serializable]
public class GameSaveData
{
    public int enemiesKilled;
    public Dictionary<string, string> unitsLinkedToDeities = new Dictionary<string, string>();

    public List<CharacterData> characterData = new List<CharacterData>();
    public int highestUnlockedLevel;

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
    public float coinsReward;
    public float experiencePointsReward;
}

