using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public List<SerializableDeity> Deities;
    public List<SerializableUnit> Units;
}

public class DeityLinkController : MonoBehaviour
{
    public void Start()
    {
    }
    public void SaveGame()
    {
        //SaveData data = new SaveData
        //{
        //    Deities = GameManager.Instance.capturedDeities.Select(deity => new SerializableDeity { Id = deity.Id }).ToList(),
        //    Units = GameManager.Instance.playerPartyMembersInstances.Select(unit => new SerializableUnit { Id = unit.Id, LinkedDeityId = unit.LinkedDeityId }).ToList()
        //};

        //string json = JsonUtility.ToJson(data, true);
        //System.IO.File.WriteAllText(Application.persistentDataPath + "/savegame.json", json);
        //Debug.Log("Game saved");
    }

    public void LoadGame()
    {
        //string path = Application.persistentDataPath + "/savegame.json";
        //if (System.IO.File.Exists(path))
        //{
        //    string json = System.IO.File.ReadAllText(path);
        //    SaveData data = JsonUtility.FromJson<SaveData>(json);

        //    // Assuming the lists are already populated with deity and unit instances
        //    foreach (var serializableUnit in data.Units)
        //    {
        //        Unit unit = GameManager.Instance.playerPartyMembersInstances.Find(u => u.Id == serializableUnit.Id);
        //        if (unit != null)
        //        {
        //            unit.linkedDeity = GameManager.Instance.capturedDeities.Find(deity => deity.Id == serializableUnit.LinkedDeityId);
        //        }
        //    }

        //    Debug.Log("Game loaded");
        //}
    }

}