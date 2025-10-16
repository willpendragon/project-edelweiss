using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] private List<string> _lootedIngredients;
    public int enemiesKilledInCurrentBattle;

    public delegate void ResetUnitUI();
    public static event ResetUnitUI OnResetUnitUI;

    public delegate void BattleEnd(string battleEndMessage);
    public static event BattleEnd OnBattleEnd;

    public delegate void BattleEndDialogueUnlock();
    public static event BattleEndDialogueUnlock OnBattleEndDialogueUnlock;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        GameObject.FindGameObjectsWithTag(Tags.PLAYER);
    }
    public void PlayerPartyVictorySequence(string battleEndPanelMessage, float receivedWarFunds)
    {
        // Execute after Player victory sequence.
        OnBattleEnd("Victory");
        BattleManager.Instance.PlayCameraBattleEndAnimation();
        ResetBattleToInitialStatus();
        battleManager.UnlockNextLevel();

        // Loops through each of the Party Units and Applies the rewards (experience, coins).

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
            }
        }
        // Add Ingredients to Persistent Inventory
        foreach (var player in TurnController.Instance.playerUnitsOnBattlefield)
        {
            var rewards = player.GetComponent<BattleRewardsController>();
            foreach (var ingredient in rewards.ingredients)
            {
                PersistentInventoryManager.CurrentInventory.Add(ingredient);
            }
            rewards.ingredients.Clear();
        }

        foreach (var entry in PersistentInventoryManager.CurrentInventory.items)
        {
            string ingredientDetails = $"{entry.ingredient.name} x{entry.quantity}";
            _lootedIngredients.Add(ingredientDetails);
        }

        BattleManager.Instance.battleRewardsController.ApplyPartyRewardsAndSave(receivedWarFunds);
        OnBattleEndDialogueUnlock();
        UpdateBattleEndUIPanel(receivedWarFunds);
    }
    public void PlayerPartyDefeatSequence()
    {
        // This is the sequence of events firing when the Player Party loses the battle.
        OnBattleEnd("Defeat");
        BattleManager.Instance.PlayCameraBattleEndAnimation();
        ResetBattleToInitialStatus();
    }

    public void ResetBattleToInitialStatus()
    {
        // I can move this in the Battle Manager
        ResetTags();
        DeactivateActivePlayerUnitPanel();
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
        battleEndUIHandler.battleEndEnemiesKilledText.text = $"Enemies Killed<space=60>{enemiesKilledInCurrentBattle}";
        battleEndUIHandler.battleEndWarFundsGainedText.text = $"War Funds Gained<space=20>{warFunds}<space=30><sprite=93>";
        battleEndUIHandler.battleEndCrystalObtainedText.text = $"Tributes<space=20>{battleManager.captureCrystalsRewardPool}<space=30><sprite=98>";

        string ingredientNames = string.Join(",", _lootedIngredients);
        battleEndUIHandler.battleEndIngredients.text = ingredientNames;

        //battleEndUIHandler.battleEndCrystalObtainedText.text = $"Capture Crystals<space=20>{battleManager.captureCrystalsRewardPool}<space=30><voffset=10><sprite=0></voffset>";
    }
}