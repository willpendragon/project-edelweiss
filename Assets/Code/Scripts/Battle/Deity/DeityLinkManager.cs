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

public class DeityLinkManager : MonoBehaviour
{
    public List<Deity> collectibleDeities;

    public void ApplyDeityLinks()
    {
        Dictionary<string, string> unitsLinkedToDeities = SaveStateManager.saveData.unitsLinkedToDeities;
        foreach (var entry in unitsLinkedToDeities)
        {
            string unitID = entry.Key;
            string deityID = entry.Value;
        }

        foreach (var unitPrefab in GameManager.Instance.playerPartyMembersInstances)
        {
            unitsLinkedToDeities.TryGetValue(unitPrefab.GetComponent<Unit>().Id, out string connectedDeity);
            unitPrefab.GetComponent<Unit>().LinkedDeityId = connectedDeity;
            unitPrefab.GetComponent<Unit>().linkedDeity = collectibleDeities.Find(deity => deity.Id == unitPrefab.LinkedDeityId);
        }
    }
}