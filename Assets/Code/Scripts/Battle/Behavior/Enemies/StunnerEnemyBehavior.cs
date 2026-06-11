using DG.Tweening;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "StunnerEnemyBehavior", menuName = "EnemyBehavior/StunnerEnemy")]
public class StunnerEnemyBehavior : EnemyBehavior
{
    [SerializeField, Range(0f, 100f)] private float stunSuccessChancePercentage = 75f; // Set this in the Inspector
    [SerializeField] private float spellVfxYOffset = 4.0f;
    // public int opportunity;

    public delegate void CheckPlayer();
    public static event CheckPlayer OnCheckPlayer;

    public delegate void StunnerEnemyAttack(string notification);
    public static event StunnerEnemyAttack OnStunnerEnemyAttack;

    [SerializeField] private GameObject attackVFXAnimator;

    private System.Random localRandom = new System.Random(); // Local random number generator

    public override void ExecuteBehavior(EnemyAgent enemyAgent)
    {
        Unit enemyUnit = enemyAgent.gameObject.GetComponentInParent<Unit>();
        Unit targetUnit = SelectTargetUnit(enemyUnit);

        if (enemyAgent.gameObject.CompareTag("DeadEnemy") && enemyUnit.currentUnitLifeCondition == Unit.UnitLifeCondition.unitDead)
        {
            OnCheckPlayer?.Invoke();
            return;
        }
        if (targetUnit == null)
        {
            OnCheckPlayer?.Invoke();
            return;
        }

        // Stun Ability triggering formula.
        if (CheckDistanceFromTarget(targetUnit, enemyUnit))
        {
            // The target IS in range (<= 3 tiles). Now we roll for success.
            float randomRoll = (float)localRandom.NextDouble() * 100f; 
            
            if (randomRoll <= stunSuccessChancePercentage)
            {
                StunAbility(targetUnit, enemyUnit);
            }
            else
            {
                // In range, but the percentage chance failed.
                OnStunnerEnemyAttack($"{enemyUnit.unitTemplate.unitName} missed the attack!");
            }
        }
        else
        {
            // The closest target is further than 3 tiles away.
            OnStunnerEnemyAttack($"{enemyUnit.unitTemplate.unitName} is too far from the target!");
        }
        
        // opportunity -= 1;
    }

    bool CheckDistanceFromTarget(Unit targetUnit, Unit enemyUnit)
    {
        if (targetUnit.ownedTile == null || enemyUnit.ownedTile == null) return false;

        int distance = GetTileDistance(targetUnit.ownedTile, enemyUnit.ownedTile);
        int range = 3;
        
        return distance <= range;
    }

    private int GetTileDistance(TileController tileA, TileController tileB)
    {
        // Calculate raw mathematical distance, completely ignoring pathfinding blockages
        return Mathf.Abs(tileA.gridPosition.x - tileB.gridPosition.x) +
               Mathf.Abs(tileA.gridPosition.y - tileB.gridPosition.y) +
               Mathf.Abs(tileA.gridPosition.z - tileB.gridPosition.z);
    }

    public Unit SelectTargetUnit(Unit enemyUnit)
    {
        GameObject[] playerUnitsOnBattlefield = GameObject.FindGameObjectWithTag("PlayerPartyController").GetComponent<PlayerPartyController>().playerUnitsOnBattlefield;

        Unit selectedUnit = playerUnitsOnBattlefield
            .Where(go => go != null)
            .Select(go => go.GetComponent<Unit>())
            .Where(unit => unit != null && unit.ownedTile != null) // Ensure they have an owned tile
            .Where(unit => 
            {
                var statusController = unit.GetComponentInChildren<UnitStatusController>();
                return statusController != null && statusController.unitCurrentStatus != UnitStatus.stun;
            })
            .OrderBy(unit => GetTileDistance(unit.ownedTile, enemyUnit.ownedTile)) // Use raw coordinate distance
            .ThenByDescending(unit => unit.unitHealthPoints)
            .FirstOrDefault();

        return selectedUnit;
    }

    public void StunAbility(Unit targetUnit, Unit enemyUnit)
    {
            OnStunnerEnemyAttack($"{enemyUnit.unitTemplate.unitName} used Stun attack");
            
            Vector3 vfxPosition = targetUnit.transform.position + new Vector3(0, spellVfxYOffset, 0);
            GameObject spellVFX = Instantiate(Resources.Load<GameObject>("VFX/StunningSpellVFX"), vfxPosition, Quaternion.identity);
            Destroy(spellVFX, 1f);

            targetUnit.GetComponentInChildren<UnitStatusController>().unitCurrentStatus = UnitStatus.stun;
            targetUnit.GetComponentInChildren<UnitStatusController>().UnitStun.Invoke();
            PlayStunFeedback(targetUnit);
    }

    private void PlayStunFeedback(Unit targetUnit)
    {
        // Define the Y offset for the VFX spawn position
        float yOffset = 1.0f;

        // Calculate the new spawn position with the Y offset
        Vector3 stunVFXSpawnPosition = targetUnit.transform.position + new Vector3(0, yOffset, 0);

        // Instantiate the VFX at the new position
        GameObject stunVFX = Instantiate(Resources.Load<GameObject>("StunAttackVFX"), stunVFXSpawnPosition, Quaternion.identity);

        // Get the duration of the VFX animation (you can set this to the actual duration of your VFX animation)
        float vfxDuration = 1.0f; // replace with the actual duration

        // Create a sequence
        Sequence sequence = DOTween.Sequence();

        // Add a delay to the sequence equal to the duration of the VFX
        sequence.AppendInterval(vfxDuration);

        // Add a callback to the sequence to instantiate the StunIcon after the delay
        sequence.AppendCallback(() =>
        {
            // Instantiate the StunIcon
            GameObject stunIconInstance = Instantiate(Resources.Load<GameObject>("StunIcon"), targetUnit.transform);
            GridManager.Instance.statusIcons.Add(stunIconInstance);

            // Create a sequence for the StunIcon animations
            Sequence iconSequence = DOTween.Sequence();

            // Add a scale up animation for the pop effect
            iconSequence.Append(stunIconInstance.transform.DOScale(new Vector3(1.5f, 1.5f, 1.5f), 0.2f).SetEase(Ease.OutBack));

            // Add a scale back to normal size
            iconSequence.Append(stunIconInstance.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack));

            // Add a shake animation
            iconSequence.Append(stunIconInstance.transform.DOShakePosition(0.5f, new Vector3(0.2f, 0.2f, 0), 10, 90, false, true));

            // Play the icon sequence
            iconSequence.Play();
        });

        float stunVFXDestroyCountdown = 1.5f;
        Destroy(stunVFX, stunVFXDestroyCountdown);
    }
}