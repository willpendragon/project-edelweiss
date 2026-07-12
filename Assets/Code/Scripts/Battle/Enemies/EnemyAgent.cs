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

    [SerializeField]
    private EnemyAITilePriority
        _enemyAITilePriority; // Specification of which type of tile the AI Unit should privilege.

    public ElementalImbue elementalImbue;
    [SerializeField] private Unit _enemyUnit;

    [Header("Presentation")]
    [SerializeField]
    float enemyMoveElapsingTime;

    [SerializeField] Animator enemyAnimator;
    [SerializeField] GameObject attackVFXAnimator;
    public Vector3 enemyOriginalPosition;

    [Header("Enemy UI")][SerializeField] TextMeshProUGUI healthPointsCounter;
    [SerializeField] TextMeshProUGUI opportunityCounter;
    [SerializeField] TextMeshProUGUI receivedDamageCounter;
    [SerializeField] SpriteRenderer enemySpriteRenderer;
    public bool isTurnComplete = false;

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

    public void StartAttackSequence(Unit targetPlayerUnit, float finalDamage, EnemyBehavior.DefenseRequirement defReq,
        System.Action onAttackComplete)
    {
        StartCoroutine(AttackSequenceRoutine(targetPlayerUnit, finalDamage, defReq, onAttackComplete));
    }

    private System.Collections.IEnumerator AttackSequenceRoutine(Unit targetPlayerUnit, float finalDamage,
        EnemyBehavior.DefenseRequirement defReq, System.Action onAttackComplete)
    {
        Unit enemyUnit = GetComponent<Unit>();

        if (defReq == EnemyBehavior.DefenseRequirement.Parryable && RealTimeActionManager.Instance != null)
        {
            RealTimeActionManager.Instance.StartWindup();
        }

        if (gameObject.GetComponentInChildren<BattleFeedbackController>() != null)
        {
            gameObject.GetComponentInChildren<BattleFeedbackController>()
                .PlayMeleeAttackAnimation(enemyUnit, targetPlayerUnit);
        }

        float timeUntilHit = 0.4f; // Tweak based on animation duration.
        yield return new WaitForSeconds(timeUntilHit);

        bool wasParried = false;

        switch (defReq)
        {
            case EnemyBehavior.DefenseRequirement.Parryable:
                bool isParryResolved = false;

                System.Action onSuccess = () =>
                {
                    wasParried = true;
                    isParryResolved = true;
                };
                System.Action onFailure = () =>
                {
                    wasParried = false;
                    isParryResolved = true;
                };

                if (RealTimeActionManager.Instance != null)
                {
                    RealTimeActionManager.Instance.OnParrySuccess += onSuccess;
                    RealTimeActionManager.Instance.OnParryFailure += onFailure;

                    RealTimeActionManager.Instance.OpenParryWindow(targetPlayerUnit);
                    // Hide the Thinking Icon to avoid obstructing Parry's warning view.
                    GetComponentInChildren<IconDisplayHelper>()?.HideIcon();
                    yield return new WaitUntil(() => isParryResolved);

                    RealTimeActionManager.Instance.OnParrySuccess -= onSuccess;
                    RealTimeActionManager.Instance.OnParryFailure -= onFailure;
                }
                else
                {
                    Debug.LogWarning("ParrySystem not assigned on " + gameObject.name);
                    wasParried = false;
                }

                break;

            case EnemyBehavior.DefenseRequirement.Unblockable:
                wasParried = false;
                break;
        }

        if (wasParried)
        {
            Debug.Log($"<color=cyan>{targetPlayerUnit.name} parried the attack!</color>");

            // Trigger parry feedback.
            if (BattleInterface.Instance != null)
                BattleInterface.Instance.SetBattleNotification("Parry!");
        }
        else
        {
            Debug.Log($"<color=red>Attack landed on {targetPlayerUnit.name}!</color>");

            targetPlayerUnit.TakeDamage(finalDamage);
            targetPlayerUnit.OnTakenDamage?.Invoke(finalDamage);

            // if (attackVFXAnimator != null)
            //     Instantiate(attackVFXAnimator, targetPlayerUnit.transform.position, Quaternion.identity);
            // Spawn Feedback (currently has the fire icon as a fallback).
        }

        onAttackComplete?.Invoke();
    }
}