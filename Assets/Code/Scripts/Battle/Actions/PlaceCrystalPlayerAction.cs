using UnityEngine;
using DG.Tweening;
using Edelweiss.Core;
using ProjectEdelweiss.Utils;

public class PlaceCrystalPlayerAction : MonoBehaviour, IPlayerAction<TileController>
{
    // The name of the class is misleading: the crystals are now known as DeityTributes.
    public int selectionLimiter = 1;
    public GameObject captureCrystal;
    public TileController currentSavedTile;

    private System.Random localRandom = new System.Random();

    private const int ManaCost = 5;

    public delegate void CaptureAttempt(string captureResult);
    public static event CaptureAttempt OnCaptureAttempt;
    public delegate void PlaceCrystal(string notification);
    public static event PlaceCrystal OnPlaceCrystal;
    public delegate void TributeUsed(int totalStacks, float totalModifier);
    public static event TributeUsed OnTributeUsed;

    private const string FAILED_CAPTURE_MESSAGE = "The binding attempt failed.";
    private const string TRIBUTE_USED_MESSAGE = "Tribute offered. Capture chance increased!";

    public void Select(TileController selectedTile)
    {
    }

    public void Execute(TileController targetTile)
    {
        // Prevents to place a Crystal outside a Deity Battle.
        var battleTypeController = FindAnyObjectByType<BattleTypeController>();
        if (battleTypeController.currentBattleType != BattleTypeController.BattleType.BattleWithDeity)
        {
            OnPlaceCrystal($"No need to use this now...");
            return;
        }

        Unit activePlayerUnit = GameObject.FindGameObjectWithTag(GameTags.ActivePlayerUnit).GetComponent<Unit>();
        GameStatsManager gameStatsManager = GameObject.FindGameObjectWithTag(GameTags.GAME_STATS_MANAGER).GetComponent<GameStatsManager>();

        if (activePlayerUnit == null || gameStatsManager == null) return;

        if (activePlayerUnit.unitOpportunityPoints <= 0) return;
        if (activePlayerUnit.unitManaPoints < ManaCost) return;
        if (gameStatsManager.captureCrystalsCount <= 0) return;
        if (targetTile.currentSingleTileCondition != SingleTileCondition.free) return;

        activePlayerUnit.SpendManaPoints(ManaCost);
        activePlayerUnit.unitOpportunityPoints--;
        UpdateActivePlayerUnitProfile(activePlayerUnit);

        gameStatsManager.captureCrystalsCount--;
        gameStatsManager.ConsumeDeityTribute();

        GameObject captureCrystalInstance = Instantiate(Resources.Load("CaptureCrystal") as GameObject, targetTile.transform.position, Quaternion.identity);

        AnimateCrystal(captureCrystalInstance);

        activePlayerUnit.GetComponent<BattleFeedbackController>().PlayPlaceCrystalSFX.Invoke();

        // Add tribute modifier stack (replaces old immediate capture attempt)
        float totalModifier = TributeModifierTracker.Instance.AddTributeStack();
        int totalStacks = TributeModifierTracker.Instance.TributeStacks;
        
        // Notify UI systems
        OnTributeUsed?.Invoke(totalStacks, totalModifier);
        OnPlaceCrystal?.Invoke($"Tribute offered (+{(totalModifier * 100):F0}% total bonus)");
        
        // Save the updated tribute count
        gameStatsManager.SaveCaptureCrystalsCount();
    }

    public void PlayFailureFeedback(GameObject deityObelisk)
    {
        if (deityObelisk == null) return;

        // Get renderer and original color
        Renderer rend = deityObelisk.GetComponentInChildren<MeshRenderer>();
        if (rend == null) return;
        Material mat = rend.material;
        Color originalColor = mat.color;

        // Create a DOTween sequence
        Sequence seq = DOTween.Sequence();

        // Shake and flash red simultaneously
        seq.Join(deityObelisk.transform.DOShakePosition(0.3f, 0.2f, 10, 90, false, true));
        seq.Join(rend.material.DOColor(Color.red, 0.1f));

        // Revert color back to original
        seq.Append(rend.material.DOColor(originalColor, 0.2f));
    }

    private void AnimateCrystal(GameObject captureCrystalInstance)
    {
        GameObject deityObeliskSpawningPoint = GameObject.FindGameObjectWithTag(GameTags.DEITY_SPAWNER).GetComponent<DeitySpawner>().DeityObeliskSpawningPoint;

        Vector3 startPos = captureCrystalInstance.transform.position;
        Vector3 endPos = deityObeliskSpawningPoint.transform.position;

        captureCrystalInstance.transform.localScale = Vector3.zero;

        Sequence crystalSequence = DOTween.Sequence();

        // Enlarge crystal (showing it)
        crystalSequence.Append(
            captureCrystalInstance.transform.DOScale(new Vector3(1.5f, 1.5f, 1.5f), 0.4f).SetEase(Ease.OutBack)
        );

        // Move Crystal to the Deity conduit.

        crystalSequence.Append(
            captureCrystalInstance.transform.DOMove(deityObeliskSpawningPoint.transform.position, 0.5f)
            );

        // Shrink (disappear) crystal
        crystalSequence.Append(
            captureCrystalInstance.transform.DOScale(Vector3.zero, 0.4f).SetEase(Ease.InBack)
        );

        crystalSequence.Play();
    }

    public void Deselect()
    {
    }

    private bool AttemptCapture()
    {
        Deity deity = GameObject.FindGameObjectWithTag(GameTags.DEITY_SPAWNER).GetComponent<DeitySpawner>().currentUnboundDeity;
        if (deity == null)
        {
            return false;
        }

        int maxHP = deity.gameObject.GetComponent<Unit>().unitTemplate.unitMaxHealthPoints;
        int currentHP = deity.gameObject.GetComponent<Unit>().unitTemplate.unitHealthPoints;
        int healthPercentage = (int)(((float)currentHP / maxHP) * 100);

        float captureProbability = 0.1f;
        switch (healthPercentage)
        {
            case <= 30:
                captureProbability = 0.6f;
                break;
            case <= 60:
                captureProbability = 0.3f;
                break;
            default:
                captureProbability = 0.1f;
                break;
        }

        float captureRoll = (float)localRandom.NextDouble();
        return captureRoll < captureProbability;
    }

    public void CreateDictionaryEntry(Deity capturedDeity, string playerId)
    {
        GameSaveData saveData = SaveStateManager.saveData;
        saveData.unitsLinkedToDeities.Add(playerId, capturedDeity.Id);
        SaveStateManager.SaveGame(saveData);
    }
    private void UpdateActivePlayerUnitProfile(Unit activePlayerUnit)
    {
        // Use centralized logic.
        //activePlayerUnit.unitProfilePanel.GetComponent<UnitProfileController>().UpdateActivePlayerProfile(activePlayerUnit);
        BattleInterface.Instance.PlayerPartyProfilesUIManager.UpdateProfile(activePlayerUnit.unitTemplate.unitName);
    }

}
