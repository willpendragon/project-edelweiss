using UnityEngine;
using DG.Tweening;
using Edelweiss.Core;
using ProjectEdelweiss.Utils;

public class PlaceCrystalPlayerAction : MonoBehaviour, IPlayerAction<TileController>
{
    public int selectionLimiter = 1;
    public GameObject captureCrystal;
    public TileController currentSavedTile;

    private System.Random localRandom = new System.Random();

    private const int ManaCost = 5;

    public delegate void BattleEndCapturedDeity(string battleEndMessage);
    public static event BattleEndCapturedDeity OnBattleEndCapturedDeity;
    public delegate void CaptureAttempt(string captureResult);
    public static event CaptureAttempt OnCaptureAttempt;
    public delegate void PlaceCrystal(string notification);
    public static event PlaceCrystal OnPlaceCrystal;

    private const string FAILED_CAPTURE_MESSAGE = "The binding attempt failed.";

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

        GameObject captureCrystalInstance = Instantiate(Resources.Load("CaptureCrystal") as GameObject, targetTile.transform.position, Quaternion.identity);

        AnimateCrystal(captureCrystalInstance, targetTile.transform.position);

        activePlayerUnit.GetComponent<BattleFeedbackController>().PlayPlaceCrystalSFX.Invoke();

        if (AttemptCapture()) // Attempt capture is successful
        {
            Deity capturedUnboundDeity = GameObject.FindGameObjectWithTag(GameTags.DEITY_SPAWNER).GetComponent<DeitySpawner>().currentUnboundDeity;
            OnBattleEndCapturedDeity?.Invoke("Deity was Captured");

            TurnController turnController = GameObject.FindGameObjectWithTag(GameTags.BATTLE_MANAGER).GetComponent<TurnController>();
            BattleFlowController.Instance.ResetTags();
            BattleManager.Instance.UnlockNextLevel();
            gameStatsManager.SaveCaptureCrystalsCount();

            string activePlayerUnitId = activePlayerUnit.GetComponent<Unit>().Id;

            CreateDictionaryEntry(capturedUnboundDeity, activePlayerUnitId);
            GameManager.Instance.DeityLinkManager.ApplyDeityLinks();
        }
        else
        {
            OnCaptureAttempt(FAILED_CAPTURE_MESSAGE);
        }
    }

    private void AnimateCrystal(GameObject captureCrystalInstance, Vector3 currentSavedTilePosition)
    {
        Deity deity = GameObject.FindGameObjectWithTag(GameTags.DEITY_SPAWNER).GetComponent<DeitySpawner>().currentUnboundDeity;

        Vector3 startPos = captureCrystalInstance.transform.position;
        Vector3 endPos = deity.transform.position;

        var energyLine = captureCrystalInstance.GetComponentInChildren<LineRenderer>();
        energyLine.positionCount = 2;
        energyLine.SetPosition(0, startPos);
        energyLine.SetPosition(1, endPos);
        energyLine.enabled = false; // Start hidden

        captureCrystalInstance.transform.localScale = Vector3.zero;

        Sequence crystalSequence = DOTween.Sequence();

        // Enlarge crystal (showing it)
        crystalSequence.Append(
            captureCrystalInstance.transform.DOScale(new Vector3(1.5f, 1.5f, 1.5f), 0.4f).SetEase(Ease.OutBack)
        );

        // Show LineRenderer
        crystalSequence.AppendCallback(() => energyLine.enabled = true);

        // Keep line for a short duration
        crystalSequence.AppendInterval(0.3f);

        // Hide line
        crystalSequence.AppendCallback(() => energyLine.enabled = false);

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
        activePlayerUnit.unitProfilePanel.GetComponent<UnitProfileController>().UpdateActivePlayerProfile(activePlayerUnit);

    }
}
