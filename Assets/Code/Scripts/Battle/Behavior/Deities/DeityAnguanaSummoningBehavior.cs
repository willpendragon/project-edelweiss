using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(fileName = "AnguanaSummonBehavior", menuName = "DeityBehavior/AnguanaSummonBehavior")]
public class DeityAnguanaSummoningBehavior : DeityBehavior
{
    // Event system for cutins
    public delegate void UsedFrozenPunishment(DeityCutinConfig config, System.Action onMoveComplete);
    public static event UsedFrozenPunishment OnUsedFrozenPunishment;

    [SerializeField] private DeityCutinConfig _cutinConfig;

    public float baseDamage = 20f;
    public string moveName = "Deity Summon Move";
    public int moveCooldown = 3;

    public override void ExecuteBehavior(Deity deity)
    {
        GameObject[] enemies = TurnController.instance.enemyUnitsOnBattlefield;

        // Roll between 1 and 12
        int roll = Random.Range(1, 13);
        int cycleDuration = (roll <= 6) ? 6 : 12;

        // Trigger cutin if configured, otherwise execute directly
        if (_cutinConfig != null && OnUsedFrozenPunishment != null)
        {
            OnUsedFrozenPunishment.Invoke(_cutinConfig, () => ExecuteFrozenPunishment(enemies, cycleDuration, deity));
        }
        else
        {
            ExecuteFrozenPunishment(enemies, cycleDuration, deity);
        }
    }

    public override void ExecuteBuffBehaviour(Deity deity, Unit linkedUnit)
    {
    }

    private void ExecuteFrozenPunishment(GameObject[] enemies, int roll, Deity deity)
    {
        if (enemies == null || enemies.Length == 0)
            return;
        int actualHitCount = Mathf.Min(roll, enemies.Length);

        Sequence summonSequence = DOTween.Sequence();

        for (int i = 0; i < actualHitCount; i++)
        {
            GameObject enemyTarget = enemies[i];

            summonSequence.AppendCallback(() =>
            {
                if (enemyTarget != null)
                {
                    ApplyHit(enemyTarget);
                }
            });
            summonSequence.AppendInterval(0.15f);
        }

        BattleInterface.Instance.SetDeityNotification(
            $"{deity.gameObject.GetComponent<Unit>().unitTemplate.unitName} used Frozen Punishment");
    }

    private void ApplyHit(GameObject enemyTarget)
    {
        Unit unit = enemyTarget.GetComponent<Unit>();
        if (unit != null)
        {
            if (unit.unitStatusController != null)
            {
                unit.unitStatusController.unitCurrentStatus = UnitStatus.stun;
            }

            unit.TakeDamage(baseDamage); //remember to add "* affinity multiplier"
            // Add freeze feedback, as this is actually a character being frozen.
            Debug.Log($"{unit.unitTemplate.unitName} hit by Frozen Punishment");
        }
    }

    public int GetMoveCooldown()
    {
        return moveCooldown;
    }
}