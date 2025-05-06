using UnityEngine;
using DG.Tweening;

public class PlaceCrystalPlayerAction : MonoBehaviour, IPlayerAction
{
    public int selectionLimiter = 1;
    public GameObject captureCrystal;
    public TileController currentSavedTile;

    private System.Random localRandom = new System.Random(); // Local random number generator

    private const int ManaCost = 5;

    public delegate void BattleEndCapturedDeity(string battleEndMessage);
    public static event BattleEndCapturedDeity OnBattleEndCapturedDeity;

    public void Select(TileController selectedTile)
    {
        if (BattleTypeController.Instance.currentBattleType == BattleTypeController.BattleType.BattleWithDeity)
        {
            Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();

            if (activePlayerUnit.unitOpportunityPoints > 0 && selectionLimiter > 0)
            {
                selectedTile.currentSingleTileStatus = SingleTileStatus.waitingForConfirmationMode;
                currentSavedTile = selectedTile;
                selectedTile.GetComponentInChildren<SpriteRenderer>().color = Color.blue;
                selectionLimiter--;
            }
        }
        else
        {
            Debug.Log("This is not a Deity Battle therefore the Player can't place Capture Crystals");
        }
        Debug.Log("Tried to Apply Crystal Tile");
    }
    public void Execute()
    {
        if (currentSavedTile.currentSingleTileStatus == SingleTileStatus.waitingForConfirmationMode)
        {
            Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();
            GameStatsManager gameStatsManager = GameObject.FindGameObjectWithTag("GameStatsManager").GetComponent<GameStatsManager>();

            if (activePlayerUnit.unitManaPoints > 0 && gameStatsManager.captureCrystalsCount > 0)
            {
                activePlayerUnit.SpendManaPoints(ManaCost);
                activePlayerUnit.unitOpportunityPoints--;

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

                if (AttemptCapture())
                {
                    Deity capturedUnboundDeity = GameObject.FindGameObjectWithTag("DeitySpawner").GetComponent<DeitySpawner>().currentUnboundDeity;
                    Debug.Log("Deity was captured");
                    OnBattleEndCapturedDeity("Deity was Captured");

                    TurnController turnController = GameObject.FindGameObjectWithTag("BattleManager").GetComponent<TurnController>();
                    BattleFlowController.Instance.ResetTags();
                    BattleManager.Instance.UnlockNextLevel();
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
        Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();
        TrapTileUIController trapTileUIController = activePlayerUnit.GetComponent<TrapTileUIController>();
        trapTileUIController.trapTileSelectionIsActive = true;

        if (currentSavedTile != null)
        {
            currentSavedTile.GetComponentInChildren<SpriteRenderer>().material.color = Color.white;
            currentSavedTile.currentSingleTileStatus = SingleTileStatus.selectionMode;
            BattleInterface.Instance.DeactivateActionInfoPanel();
            Debug.Log("Deselecting Currently Selected Tile");
            currentSavedTile.GetComponentInChildren<SpriteRenderer>().color = Color.white;
            currentSavedTile = null;
            selectionLimiter++;
        }
        else if (currentSavedTile == null)
        {
            BattleInterface.Instance.DeactivateActionInfoPanel();
            foreach (var tile in GridManager.Instance.gridTileControllers)
            {
                tile.currentPlayerAction = new SelectUnitPlayerAction();
                tile.tileShaderController.AnimateFadeHeight(0, 0.2f, Color.white);
            }
            GameObject[] playerUISpellButtons = GameObject.FindGameObjectsWithTag("PlayerUISpellButton");
            foreach (var playerUISpellButton in playerUISpellButtons)
            {
                Destroy(playerUISpellButton);
            }
            Destroy(GridManager.Instance.currentPlayerUnit.GetComponent<Unit>().unitProfilePanel);
            GridManager.Instance.currentPlayerUnit.tag = "Player";
            GridManager.Instance.currentPlayerUnit = null;

            GameObject movesContainer = GameObject.FindGameObjectWithTag("MovesContainer");
            movesContainer.transform.localScale = new Vector3(0, 0, 0);
            Destroy(GameObject.FindGameObjectWithTag("ActivePlayerCharacterSelectionIcon"));
            GridManager.Instance.ClearPath();
            BattleInterface.Instance.DeactivateActionInfoPanel();
            selectionLimiter++;
        }
    }
    private bool AttemptCapture()
    {
        Deity deity = GameObject.FindGameObjectWithTag("DeitySpawner").GetComponent<DeitySpawner>().currentUnboundDeity;
        if (deity == null)
        {
            Debug.LogError("No deity found for capture attempt.");
            return false;
        }

        int maxHP = deity.gameObject.GetComponent<Unit>().unitTemplate.unitMaxHealthPoints;
        int currentHP = deity.gameObject.GetComponent<Unit>().unitTemplate.unitHealthPoints;
        int healthPercentage = (int)(((float)currentHP / maxHP) * 100);

        // Calculate the capture probability
        float captureProbability = 0.1f; // Default probability
        switch (healthPercentage)
        {
            case <= 30:
                captureProbability = 0.6f; // 60% chance to capture if below 30% HP
                break;
            case <= 60:
                captureProbability = 0.3f; // 30% chance to capture if between 31% and 60% HP
                break;
            default:
                captureProbability = 0.1f; // 10% chance to capture if above 60% HP
                break;
        }
        // Generate a random number between 0 and 1
        float captureRoll = (float)localRandom.NextDouble();
        // Return true if captureRoll is less than captureProbability, indicating a successful capture
        return captureRoll < captureProbability;
    }
    public void CreateDictionaryEntry(Deity capturedDeity)
    {
        string activePlayerUnitId = GridManager.Instance.currentPlayerUnit.GetComponent<Unit>().Id;
        GameSaveData saveData = SaveStateManager.saveData;
        saveData.unitsLinkedToDeities.Add(activePlayerUnitId, capturedDeity.Id);
        SaveStateManager.SaveGame(saveData);
    }
}