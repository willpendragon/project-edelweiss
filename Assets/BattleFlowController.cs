using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TurnController;
using static Unit;

public class BattleFlowController : MonoBehaviour
{
    public static BattleFlowController Instance;

    [Header("Dependencies")]
    [SerializeField] private BattleManager battleManager;
    [SerializeField] private GameStatsManager gameStatsManager;
    [SerializeField] private BattleEndUIHandler battleEndUIHandler;
    [SerializeField] private SummonResetHelper summonResetHelper;
    public int enemiesKilledInCurrentBattle;

    public delegate void ResetUnitUI();
    public static event ResetUnitUI OnResetUnitUI;

    public delegate void BattleEnd(string battleEndMessage);
    public static event BattleEnd OnBattleEnd;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject); // Prevent duplicates
    }

    private void Start()
    {
        GameObject.FindGameObjectsWithTag(Tags.PLAYER);
    }
    public void PlayerPartyVictorySequence(string battleEndPanelMessage, float receivedWarFunds)
    {
        // Execute the sequence of events firing when the Player Party wins the battle.
        OnBattleEnd("Victory");
        BattleManager.Instance.PlayCameraBattleEndAnimation();
        ResetBattleToInitialStatus();
        battleManager.UnlockNextLevel();
        Debug.Log("Enemy Party was defeated");

        foreach (var player in TurnController.Instance.playerUnitsOnBattlefield)
        {
            player.GetComponent<BattleRewardsController>().ApplyRewardsToThisUnit();
            receivedWarFunds += player.GetComponent<Unit>().unitCoins;
        }

        foreach (var enemy in BattleManager.Instance.enemiesOnBattlefield)
        {
            if (enemy.tag == Tags.ENEMY && enemy.GetComponent<Unit>().currentUnitLifeCondition == UnitLifeCondition.unitDead)
            {
                enemiesKilledInCurrentBattle++;
                // Increases enemy kill counter for UI display.
                gameStatsManager.enemiesKilled++;
                Debug.Log("Adding enemies to kill count");
            }
        }
        BattleManager.Instance.battleRewardsController.ApplyPartyRewardsAndSave(receivedWarFunds);
        ConversationManager.Instance.UnlockRandomConversation();
        Debug.Log("Rolled Convo Unlock");
        UpdateBattleEndUIPanel(receivedWarFunds);
    }
    public void PlayerPartyDefeatSequence()
    {
        // This is the sequence of events firing when the Player Party wins the battle.
        OnBattleEnd("Defeat");
        BattleManager.Instance.PlayCameraBattleEndAnimation();
        ResetBattleToInitialStatus();
        Debug.Log("Player Party was defeated");
    }

    public void ResetBattleToInitialStatus()
    {
        // I can move this in the Battle Manager
        ResetTags();
        DeactivateActivePlayerUnitPanel();
        OnResetUnitUI();
        summonResetHelper.ResetSummonTemporaryBuffs();
    }
    public void ResetTags()
    {
        foreach (var player in GameManager.Instance.playerPartyMembersInstances)
        {
            player.gameObject.tag = Tags.PLAYER;
        }
    }
    private void DeactivateActivePlayerUnitPanel()
    {
        Destroy(GameObject.FindGameObjectWithTag(Tags.ACTIVE_CHARACTER_UNIT_PROFILE));
    }
    public void UpdateBattleEndUIPanel(float warFunds)
    {
        // It should be handled by UI behaviour.
        battleEndUIHandler.battleEndEnemiesKilledText.text = enemiesKilledInCurrentBattle.ToString();
        battleEndUIHandler.battleEndWarFundsGainedText.text = warFunds.ToString();
        battleEndUIHandler.battleEndCrystalObtainedText.text = battleManager.captureCrystalsRewardPool.ToString();
    }
}