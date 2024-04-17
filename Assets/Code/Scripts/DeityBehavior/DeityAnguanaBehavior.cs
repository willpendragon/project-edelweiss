using UnityEngine;

[CreateAssetMenu(fileName = "AnguanaBehavior", menuName = "DeityBehavior/Anguana")]
public class DeityAnguanaBehavior : DeityBehavior
{
    public int deityPrayerPowerMinimumRequirement = 3;
    public int vfxDurationDelay = 3;
    public override void ExecuteBehavior(Deity deity)
    {
        BattleManager battleManager = GameObject.FindGameObjectWithTag("BattleManager").GetComponent<BattleManager>();

        if (battleManager.currentBattleType == BattleType.regularBattle)
        {
            if (deity.deityPrayerPower > deityPrayerPowerMinimumRequirement)
            {
                Debug.Log("Attacking Enemies");
            }
            else if (deity.PerformDeityEnmityCheck())
            {
                Attack(deity);
            }
            else
            {
                Debug.Log("Deity Anguana doesn't do anything");
            }
        }
        else if (battleManager.currentBattleType == BattleType.battleWithDeity)
        {
            Attack(deity);
        }
    }

    public void Attack(Deity deity)
    {
        GameObject newDeityAttackVFX = Instantiate(deity.deityAttackVFX, deity.transform);
        Destroy(newDeityAttackVFX, vfxDurationDelay);
        GameObject[] playerUnitsOnBattlefield = GameObject.FindGameObjectWithTag("PlayerPartyController").GetComponent<PlayerPartyController>().playerUnitsOnBattlefield;
        foreach (var playerUnit in playerUnitsOnBattlefield)
        {
            playerUnit.GetComponent<Unit>().TakeDamage(deity.deitySpecialAttackPower);
        }
        deity.enmity = 0;
        deity.deityEnmityTracker.GetComponent<DeityEnmityTrackerController>().UpdateDeityEnmityTracker();
        Debug.Log("Anguana Executes Spiteful Wave attack");
    }
}