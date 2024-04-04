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
    public int unitId;
    public int hp;
    public string name;
    public string description;

}

[System.Serializable]

public class ResourceData

{
    public float coins;
}

