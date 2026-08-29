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
        OnBattleEnd(battleEndPanelMessage);
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

                // Track targeted bounty kills!
                string deadUnitName = enemy.GetComponent<Unit>().unitTemplate.unitName;
                if (!string.IsNullOrEmpty(deadUnitName))
                {
                    if (SaveStateManager.saveData.killsByEnemyName == null)
                        SaveStateManager.saveData.killsByEnemyName = new Dictionary<string, int>();

                    if (SaveStateManager.saveData.killsByEnemyName.ContainsKey(deadUnitName))
                    {
                        SaveStateManager.saveData.killsByEnemyName[deadUnitName]++;
                    }
                    else
                    {
                        SaveStateManager.saveData.killsByEnemyName.Add(deadUnitName, 1);
                    }
                }
            }
        }
        // Add Ingredients to Persistent Inventory (and collect for battle-end display)
        List<Ingredient> battleLootedIngredients = new List<Ingredient>();
        foreach (var player in TurnController.Instance.playerUnitsOnBattlefield)
        {
            var rewards = player.GetComponent<BattleRewardsController>();
            foreach (var ingredient in rewards.ingredients)
            {
                PersistentInventoryManager.CurrentInventory.Add(ingredient);
                battleLootedIngredients.Add(ingredient);
            }
            rewards.ingredients.Clear();
        }

        // Show only the ingredients looted THIS battle (aggregated by type)
        Dictionary<string, int> ingredientCounts = new Dictionary<string, int>();
        foreach (var ingredient in battleLootedIngredients)
        {
            if (ingredientCounts.ContainsKey(ingredient.ingredientName))
            {
                ingredientCounts[ingredient.ingredientName]++;
            }
            else
            {
                ingredientCounts[ingredient.ingredientName] = 1;
            }
        }

        foreach (var kvp in ingredientCounts)
        {
            string ingredientDetails = $"{kvp.Key} x{kvp.Value}";
            _lootedIngredients.Add(ingredientDetails);
        }

        // ApplyPartyRewardsAndSave will naturally trigger SaveStateManager.SaveGame(), saving our modified dictionary!
        BattleManager.Instance.battleRewardsController.ApplyPartyRewardsAndSave(receivedWarFunds);
        OnBattleEndDialogueUnlock();
        UpdateBattleEndUIPanel(receivedWarFunds);
    }
    public void PlayerPartyDefeatSequence()
    {
        // This is the sequence of events firing when the Player Party loses the battle.
        OnBattleEnd("Defeat");
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
        //battleEndUIHandler.battleEndCrystalObtainedText.text = $"Tributes<space=20>{battleManager.captureCrystalsRewardPool}<space=30><sprite=98>";

        string ingredientNames = string.Join("\n", _lootedIngredients);
        battleEndUIHandler.battleEndIngredients.text = ingredientNames;

        //battleEndUIHandler.battleEndCrystalObtainedText.text = $"Capture Crystals<space=20>{battleManager.captureCrystalsRewardPool}<space=30><voffset=10><sprite=0></voffset>";
    }
}