using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public List<Deity> Deities;
    public List<Unit> Units;
    public List<Unit> UnitInstances;
}

public class DeityLinkController : MonoBehaviour
{
    public void Start()
    {
        LoadGame();
    }
    public void SaveGame()
    {
        // Assuming GameManager.Instance.Units contains all your game's units
        SaveData data = new SaveData
        {
            Deities = GameManager.Instance.capturedDeities,
            Units = GameManager.Instance.playerPartyMembersInstances
        };

        string json = JsonUtility.ToJson(data, true); // 'true' for pretty print if desired
        System.IO.File.WriteAllText(Application.persistentDataPath + "/savegame.json", json);
        Debug.Log("Saving Deity Link");
    }

    public void LoadGame()
    {
        string path = Application.persistentDataPath + "/savegame.json";
        if (System.IO.File.Exists(path))
        {
            string json = System.IO.File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            GameManager.Instance.capturedDeities = data.Deities;
            GameManager.Instance.playerPartyMembersInstances = data.Units;

            // Re-link units to deities based on saved DeityId
            foreach (var unit in GameManager.Instance.playerPartyMembersInstances)
            {
                GameObject unitInstance = Instantiate(unit.gameObject);
                unitInstance.GetComponent<Unit>().linkedDeity = GameManager.Instance.capturedDeities.Find(deity => deity.Id == unit.LinkedDeityId);
            }
            Debug.Log("Loading Deity Data");
        }
    }
}