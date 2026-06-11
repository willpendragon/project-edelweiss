using UnityEngine;
using TMPro;
public class EnemyAgent : MonoBehaviour
{
    public enum ElementalImbue
    {
        None,
        IceImbue,
        LightningImbue,
        FireImbue
    }

    [Header("Unit Statistics")]
    // To be reworked. Should uniform and make the Enemy take the statistics from its own Scriptable Object template.
    public int speed;
    public float attackPower = 60;

    [Header("Gameplay Logic")]
    // To be reworked. Player is a Unit now.
    public int opportunity;
    [SerializeField] BattleManager battleManager;
    [SerializeField] int minEnemyMoveRollRange;
    [SerializeField] int maxEnemyMoveRollRange;
    public EnemyBehavior enemyBehavior;
    [SerializeField] private EnemyAIPriority _enemyAIPriority;
    [SerializeField] private EnemyAITilePriority _enemyAITilePriority; // Specification of which type of tile the AI Unit should privilege.
    public ElementalImbue elementalImbue;
    [SerializeField] private Unit _enemyUnit;

    [Header("Presentation")]
    [SerializeField] float enemyMoveElapsingTime;
    [SerializeField] Animator enemyAnimator;
    [SerializeField] GameObject attackVFXAnimator;
    public Vector3 enemyOriginalPosition;

    [Header("Enemy UI")]
    [SerializeField] TextMeshProUGUI healthPointsCounter;
    [SerializeField] TextMeshProUGUI opportunityCounter;
    [SerializeField] TextMeshProUGUI receivedDamageCounter;
    [SerializeField] SpriteRenderer enemySpriteRenderer;

    public delegate void CheckPlayer();
    public static event CheckPlayer OnCheckPlayer;

    public delegate void CheckEnemiesOnBattlefield();
    public static event CheckEnemiesOnBattlefield OnCheckEnemiesOnBattlefield;

    public GameObject unitStunStatusIcon;

    public EnemyAIPriority EnemyAIPriority => _enemyAIPriority;
    public EnemyAITilePriority EnemyAITilePriority => _enemyAITilePriority;

    // Starts the Enemy Turn Sequence contained in the Scriptable Object.
    public void EnemyTurnEvents()
    {
        enemyBehavior.ExecuteBehavior(this);
    }

    // Elemental Tiles handling.

    public void ReceiveElement(TileController imbuedTile, EnemyAgent enemyAgent)
    {
        switch (imbuedTile.tileElement)
        {
            case TileElement.Ice:
                enemyAgent.elementalImbue = EnemyAgent.ElementalImbue.IceImbue;
                BuffEnemy(enemyAgent); // Apply Modifiers
                // Change aspect (rock becomes imbued with elemental power)
                SwapGraphics();
                break;
                // Specify other cases to create more classes with different elements.
        }
    }

    // Buffing/Debuffing handling.

    private void BuffEnemy(EnemyAgent enemyAgent)
    {
        Unit enemyUnit = GetComponent<Unit>();
        var elementalModifier = enemyUnit.unitTemplate.GetElementalModifier();
        enemyUnit.unitMeleeAttackBaseDamage = enemyUnit.unitAttackPower * elementalModifier;
    }
    private void SwapGraphics()
    {
        // This method changes the Unit sprite to an alternate model.
        var alternateSprite = _enemyUnit.unitTemplate.GetAlternateSprite();
        enemySpriteRenderer.sprite = alternateSprite;

    }

    // Should be generalized for all types of Buffs
    public void RemoveElementalBuff(EnemyAgent enemyAgent)
    {
        if (elementalImbue != ElementalImbue.None)
        {
            Unit enemyUnit = enemyAgent.GetComponent<Unit>();
            enemyUnit.unitMeleeAttackBaseDamage = enemyUnit.unitTemplate.meleeAttackPower;
            BattleInterface.Instance.SetBattleNotification($"{enemyUnit.name} debuffed");
        }
    }
}