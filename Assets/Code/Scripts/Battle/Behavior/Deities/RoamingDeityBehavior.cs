using UnityEngine;

[CreateAssetMenu(fileName = "RoamingDeityBehavior", menuName = "DeityBehavior/Roaming")]
public class RoamingDeityBehavior : DeityBehavior
{
    private string deityName = "Roaming Deity";

    [Header("Zap Attack")]
    public string zapAttackName = "Zap";
    public GameObject zapAttackVFX;
    public float vfxDurationDelay = 1f;

    [Header("Modified Zap Attack (when enmity is full)")]
    public float modifiedZapMultiplier = 2f;

    public override void ExecuteBehavior(Deity deity)
    {
        // Only zaps players; the modified zap replaces the normal zap while enmity is full
        if (deity.PerformDeityEnmityCheck())
        {
            ModifiedZapAttack(deity);
            deity.ResetDeityEnmity();
            return;
        }

        ZapAttack(deity);
    }

    public override void ExecuteBuffBehaviour(Deity deity, Unit unit)
    {

    }

    private void ZapAttack(Deity deity)
    {
        Debug.Log($"{deityName} used {zapAttackName}!");
        BattleInterface.Instance.SetDeityNotification($"{deityName} used {zapAttackName}!");
        deity.deityCry.Play();

        float baseDamage = deity.deitySpecialAttackPower;
        DealZapDamage(deity, baseDamage);
    }

    private void ModifiedZapAttack(Deity deity)
    {
        Debug.Log($"{deityName} used a rageful {zapAttackName}!");
        BattleInterface.Instance.SetDeityNotification($"{deityName} used a rageful {zapAttackName}!");
        deity.deityCry.Play();

        float modifiedDamage = deity.deitySpecialAttackPower * modifiedZapMultiplier;
        DealZapDamage(deity, modifiedDamage);
    }

    private void DealZapDamage(Deity deity, float damage)
    {
        GameObject[] playerUnitsOnBattlefield = GameObject.FindGameObjectWithTag("PlayerPartyController")
            .GetComponent<PlayerPartyController>().playerUnitsOnBattlefield;

        foreach (var playerUnitGO in playerUnitsOnBattlefield)
        {
            Unit playerUnit = playerUnitGO.GetComponent<Unit>();
            if (playerUnit != null && playerUnit.currentUnitLifeCondition == Unit.UnitLifeCondition.unitAlive)
            {
                if (zapAttackVFX != null)
                {
                    GameObject newDeityAttackVFX = Instantiate(zapAttackVFX, playerUnit.ownedTile.transform.position,
                        Quaternion.identity);
                    Vector3 attackVFXOffset = new Vector3(0, 1, 0);
                    newDeityAttackVFX.transform.localPosition += attackVFXOffset;
                    Destroy(newDeityAttackVFX, vfxDurationDelay);
                }

                playerUnit.TakeDamage(damage);
                Debug.Log($"Damaged {playerUnit.unitTemplate.unitName} for {damage}");
            }
        }
    }
}
