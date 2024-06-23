using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

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
        if (BattleManager.Instance.currentBattleType == BattleType.battleWithDeity)
        {
            Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();

            if (activePlayerUnit.unitOpportunityPoints > 0 && selectionLimiter > 0)
            {
                selectedTile.currentSingleTileStatus = SingleTileStatus.waitingForConfirmationMode;
                currentSavedTile = selectedTile;
                selectedTile.GetComponentInChildren<SpriteRenderer>().color = Color.blue;
                activePlayerUnit.SpendManaPoints(ManaCost);
                activePlayerUnit.unitOpportunityPoints--;
                selectionLimiter--;
            }
        }
        else
        {
            Debug.Log("This is not a Deity Battle therefore the Player can't place Capture Crystals");
        }
    }

    public void Execute()
    {
        if (currentSavedTile.currentSingleTileStatus == SingleTileStatus.waitingForConfirmationMode)
        {
            Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();
            GameStatsManager gameStatsManager = GameObject.FindGameObjectWithTag("GameStatsManager").GetComponent<GameStatsManager>();

            if (activePlayerUnit.unitManaPoints > 0 && gameStatsManager.captureCrystalsCount > 0)
            {
                GameObject captureCrystalInstance = Instantiate(Resources.Load("CaptureCrystal") as GameObject, currentSavedTile.transform.position, Quaternion.identity);
                gameStatsManager.captureCrystalsCount--;

                GameObject[] playerUISpellButtons = GameObject.FindGameObjectsWithTag("PlayerUISpellButton");
                foreach (var playerUISpellButton in playerUISpellButtons)
                {
                    CapsuleCrystalCounterHandler handler = playerUISpellButton.GetComponent<CapsuleCrystalCounterHandler>();
                    if (handler != null)
                    {
                        handler.UpdateCapsuleCounterText();
                    }
                }
                AnimateCrystal(captureCrystalInstance, currentSavedTile.transform.position);

                Debug.Log("Placing Crystal, attempting to Capture the Deity");

                activePlayerUnit.GetComponent<BattleFeedbackController>().PlayPlaceCrystalSFX.Invoke();
                activePlayerUnit.unitProfilePanel.GetComponent<PlayerProfileController>().UpdateActivePlayerProfile(activePlayerUnit);

                if (DeityCaptureRoll() > CaptureDifficulty)
                {
                    Deity capturedUnboundDeity = GameObject.FindGameObjectWithTag("DeitySpawner").GetComponent<DeitySpawner>().currentUnboundDeity;
                    Debug.Log("Deity was captured");
                    OnBattleEndCapturedDeity("Deity was Captured");

                    TurnController turnController = GameObject.FindGameObjectWithTag("BattleManager").GetComponent<TurnController>();
                    turnController.ResetTags();
                    turnController.UnlockNextLevel();
                    gameStatsManager.SaveCaptureCrystalsCount();


                    CreateDictionaryEntry(capturedUnboundDeity);
                    GameManager.Instance.ApplyDeityLinks();
                }
                else
                {
                    Debug.Log("Deity was not captured");
                }
            }
        }
    }

    private void AnimateCrystal(GameObject captureCrystalInstance, Vector3 currentSavedTilePosition)
    {
        // Instantiate the crystal at the initial small size and initial position
        captureCrystalInstance.transform.localScale = Vector3.zero;

        // Define the sequence of animations
        Sequence crystalSequence = DOTween.Sequence();

        // Step 1: Move up slightly while staying small
        crystalSequence.Append(captureCrystalInstance.transform.DOMoveY(currentSavedTilePosition.y + 2, 0.5f).SetEase(Ease.OutQuad));

        // Step 2: Scale up a bit and move down to original Y position
        crystalSequence.Append(captureCrystalInstance.transform.DOScale(new Vector3(1.5f, 1.5f, 1.5f), 1f).SetEase(Ease.OutQuad))
                       .Join(captureCrystalInstance.transform.DOMoveY(currentSavedTilePosition.y, 1f).SetEase(Ease.OutQuad));

        // Step 3: Scale down to the real size
        crystalSequence.Append(captureCrystalInstance.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.InOutQuad));

        // Play the sequence
        crystalSequence.Play();
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