using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using Unity.VisualScripting;

public class PlaceCrystalPlayerAction : MonoBehaviour, IPlayerAction
{
    public int selectionLimiter = 1;
    public GameObject captureCrystal;
    public TileController currentSavedTile;

    public delegate void BattleEndCapturedDeity(string battleEndMessage);
    public static event BattleEndCapturedDeity OnBattleEndCapturedDeity;

    [Serialize]

    Dictionary<string, string> capturedDeities = new Dictionary<string, string>();

    public void Select(TileController selectedTile)
    {
        Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();

        if (activePlayerUnit.unitOpportunityPoints > 0 && selectionLimiter > 0)
        {
            selectedTile.currentSingleTileStatus = SingleTileStatus.waitingForConfirmationMode;
            currentSavedTile = selectedTile;
            activePlayerUnit.SpendManaPoints(20);
            //Warning: Magic Numbers
            activePlayerUnit.unitOpportunityPoints--;
            selectionLimiter--;
        }
    }
    public void Execute()
    {
        if (currentSavedTile.currentSingleTileStatus == SingleTileStatus.waitingForConfirmationMode)
        {
            GameObject captureCrystalInstance = Instantiate(Resources.Load("CaptureCrystal") as GameObject, currentSavedTile.transform.position, Quaternion.identity);
            Debug.Log("Attempt to Capture the Deity");
            if (DeityCaptureRoll() > 10)
            //Beware, Magic Number
            {
                Deity capturedUnboundDeity = GameObject.FindGameObjectWithTag("DeitySpawner").GetComponent<DeitySpawner>().currentUnboundDeity;
                //GameManager.Instance.capturedDeities.Add(capturedUnboundDeity);
                Debug.Log("Deity was captured");
                OnBattleEndCapturedDeity("Deity was Captured");

                //Initiate the Deity captured sequence
                //GameObject.FindGameObjectWithTag("PlayerStatsSaver").GetComponent<PlayerStatsSaver>().SaveDeityData(GameManager.Instance.capturedDeities);
                CreateDictionaryEntry(capturedUnboundDeity);
            }
            else
            {
                Debug.Log("Deity was not captured");
            }
            //Insert the logic for Capturing the Deity here.
            //I'll probably need to create a class for handling the Crystal entirely and retrieve data from a Scriptable Object
        }
    }

    public void Deselect()
    {

    }

    public int DeityCaptureRoll()
    {
        int deityCaptureRoll = UnityEngine.Random.Range(0, 11);
        //Beware, Magic Number
        GameObject[] captureCrystalsOnBattlefield = GameObject.FindGameObjectsWithTag("CaptureCrystal");
        deityCaptureRoll = deityCaptureRoll * captureCrystalsOnBattlefield.Length;
        return deityCaptureRoll;
    }

    public void CreateDictionaryEntry(Deity capturedDeity)
    {
        // Creates an association suitable for the Dictionary between the Active Player Unit (who's capturing the Deity) and the Captured Deity.
        // Assume GridManager.Instance.currentPlayerUnit.GetComponent<Unit>().Id uniquely identifies your player unit
        string activePlayerUnitId = GridManager.Instance.currentPlayerUnit.GetComponent<Unit>().Id;

        // Map this player unit ID to the captured deity's ID
        capturedDeities[activePlayerUnitId] = capturedDeity.Id;

        // Now save this updated dictionary to your file
        SaveDictionaryToFile(capturedDeities);
    }

    public void SaveDictionaryToFile(Dictionary<string, string> newCapturedDeities)
    {
        string filePath = Application.persistentDataPath + "/savegame.json";
        Dictionary<string, string> existingDeities = new Dictionary<string, string>();

        // Check if the file exists
        if (File.Exists(filePath))
        {
            // Read the existing content of the file
            string jsonContent = File.ReadAllText(filePath);
            existingDeities = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonContent) ?? new Dictionary<string, string>();
        }

        // Update the dictionary with new entries
        foreach (var entry in newCapturedDeities)
        {
            // If the key already exists, it updates; otherwise, it adds a new entry
            existingDeities[entry.Key] = entry.Value;
        }

        // Serialize and save back to the file
        string listOfCapturedDeities = JsonConvert.SerializeObject(existingDeities, Formatting.Indented);
        File.WriteAllText(filePath, listOfCapturedDeities);

        Debug.Log("Updated Dictionary with Captured Deity");
    }
}
