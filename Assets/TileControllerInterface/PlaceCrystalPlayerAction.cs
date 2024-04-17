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

    private System.Random localRandom = new System.Random(); // Local random number generator

    private const int ManaCost = 20;
    private const int CaptureDifficulty = 20; // Adjusted for a low to medium capture probability
    private const int MaxCaptureRoll = 11; // Max roll value to determine capture outcome

    public delegate void BattleEndCapturedDeity(string battleEndMessage);
    public static event BattleEndCapturedDeity OnBattleEndCapturedDeity;

    public void Select(TileController selectedTile)
    {
        Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();

        if (activePlayerUnit.unitOpportunityPoints > 0 && selectionLimiter > 0)
        {
            selectedTile.currentSingleTileStatus = SingleTileStatus.waitingForConfirmationMode;
            currentSavedTile = selectedTile;
            activePlayerUnit.SpendManaPoints(ManaCost);
            activePlayerUnit.unitOpportunityPoints--;
            selectionLimiter--;
        }
    }

    public void Execute()
    {
        if (currentSavedTile.currentSingleTileStatus == SingleTileStatus.waitingForConfirmationMode)
        {
            Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();

            if (activePlayerUnit.unitManaPoints > 0)
            {
                GameObject captureCrystalInstance = Instantiate(Resources.Load("CaptureCrystal") as GameObject, currentSavedTile.transform.position, Quaternion.identity);
                Debug.Log("Attempt to Capture the Deity");
                activePlayerUnit.unitProfilePanel.GetComponent<PlayerProfileController>().UpdateActivePlayerProfile(activePlayerUnit);
                if (DeityCaptureRoll() > CaptureDifficulty)
                {
                    Deity capturedUnboundDeity = GameObject.FindGameObjectWithTag("DeitySpawner").GetComponent<DeitySpawner>().currentUnboundDeity;
                    Debug.Log("Deity was captured");
                    OnBattleEndCapturedDeity("Deity was Captured");

                    TurnController turnController = GameObject.FindGameObjectWithTag("BattleManager").GetComponent<TurnController>();
                    turnController.ResetTags();
                    turnController.UnlockNextLevel();

                    CreateDictionaryEntry(capturedUnboundDeity);
                }
                else
                {
                    Debug.Log("Deity was not captured");
                }
            }
        }
    }

    public void Deselect()
    {
        // Logic to handle deselection if needed
    }

    public int DeityCaptureRoll()
    {
        int deityCaptureRoll = localRandom.Next(0, MaxCaptureRoll);
        GameObject[] captureCrystalsOnBattlefield = GameObject.FindGameObjectsWithTag("CaptureCrystal");
        deityCaptureRoll = deityCaptureRoll * captureCrystalsOnBattlefield.Length;
        return deityCaptureRoll;
    }

    public void CreateDictionaryEntry(Deity capturedDeity)
    {
        string activePlayerUnitId = GridManager.Instance.currentPlayerUnit.GetComponent<Unit>().Id;
        GameSaveData saveData = SaveStateManager.saveData;
        saveData.unitsLinkedToDeities.Add(activePlayerUnitId, capturedDeity.Id);
        SaveStateManager.SaveGame(saveData);
    }
}