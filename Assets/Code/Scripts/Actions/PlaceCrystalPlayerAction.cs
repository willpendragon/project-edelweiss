using UnityEngine;
using DG.Tweening;
using System.Data.Common;

public class PlaceCrystalPlayerAction : MonoBehaviour, IPlayerAction
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

    private const string FAILED_CAPTURE_MESSAGE = "The capture attempt failed...";
    private const string ACTIVE_PLAYER_UNIT = "ActivePlayerUnit";

    public void Select(TileController selectedTile)
    {
    }

    public void Execute(TileController targetTile)
    {
        Unit activePlayerUnit = GameObject.FindGameObjectWithTag(ACTIVE_PLAYER_UNIT).GetComponent<Unit>();
        GameStatsManager gameStatsManager = GameObject.FindGameObjectWithTag("GameStatsManager").GetComponent<GameStatsManager>();

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

        GameObject[] playerUISpellButtons = GameObject.FindGameObjectsWithTag("PlayerUISpellButton");
        foreach (var playerUISpellButton in playerUISpellButtons)
        {
            CapsuleCrystalCounterHandler handler = playerUISpellButton.GetComponent<CapsuleCrystalCounterHandler>();
            if (handler != null)
            {
                handler.UpdateCapsuleCounterText();
            }
        }

        AnimateCrystal(captureCrystalInstance, targetTile.transform.position);
        activePlayerUnit.GetComponent<BattleFeedbackController>().PlayPlaceCrystalSFX.Invoke();

        if (AttemptCapture())
        {
            Deity capturedUnboundDeity = GameObject.FindGameObjectWithTag("DeitySpawner").GetComponent<DeitySpawner>().currentUnboundDeity;
            OnBattleEndCapturedDeity?.Invoke("Deity was Captured");

            TurnController turnController = GameObject.FindGameObjectWithTag("BattleManager").GetComponent<TurnController>();
            BattleFlowController.Instance.ResetTags();
            BattleManager.Instance.UnlockNextLevel();
            gameStatsManager.SaveCaptureCrystalsCount();

            string activePlayerUnitId = activePlayerUnit.GetComponent<Unit>().Id;

            CreateDictionaryEntry(capturedUnboundDeity, activePlayerUnitId);
            GameManager.Instance.ApplyDeityLinks();
        }
        else
        {
            OnCaptureAttempt(FAILED_CAPTURE_MESSAGE);
        }
    }

    private void AnimateCrystal(GameObject captureCrystalInstance, Vector3 currentSavedTilePosition)
    {
        captureCrystalInstance.transform.localScale = Vector3.zero;
        Sequence crystalSequence = DOTween.Sequence();
        crystalSequence.Append(captureCrystalInstance.transform.DOMoveY(currentSavedTilePosition.y + 2, 0.5f).SetEase(Ease.OutQuad));
        crystalSequence.Append(captureCrystalInstance.transform.DOScale(new Vector3(1.5f, 1.5f, 1.5f), 1f).SetEase(Ease.OutQuad))
                       .Join(captureCrystalInstance.transform.DOMoveY(currentSavedTilePosition.y, 1f).SetEase(Ease.OutQuad));
        crystalSequence.Append(captureCrystalInstance.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.InOutQuad));
        crystalSequence.Play();
    }

    public void Deselect()
    {
    }

    private bool AttemptCapture()
    {
        Deity deity = GameObject.FindGameObjectWithTag("DeitySpawner").GetComponent<DeitySpawner>().currentUnboundDeity;
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
