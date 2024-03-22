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
        GameSaveData saveData = SaveStateManager.saveData;
        saveData.unitsLinkedToDeities.Add(activePlayerUnitId, capturedDeity.Id);
        SaveStateManager.SaveGame(saveData);
    }
}
