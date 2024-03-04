[System.Serializable]
public class SerializableDeity
{
    public string Id;
    // Add other deity fields that need to be saved.
}

[System.Serializable]
public class SerializableUnit
{
    public string Id;
    public string LinkedDeityId;
    // Add other unit fields that need to be saved.
}