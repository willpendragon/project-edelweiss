using UnityEngine;

[CreateAssetMenu(fileName = "AnguanaBehavior", menuName = "DeityBehavior/Anguana")]
public class DeityAnguanaBehavior : DeityBehavior
{
    public float vfxDurationDelay = 1f;
    private string deityName = "Anguana";
    public string attackName;
    public override void ExecuteBehavior(Deity deity)
    {
        BattleManager battleManager = GameObject.FindGameObjectWithTag("BattleManager").GetComponent<BattleManager>();

        if (BattleTypeController.Instance.currentBattleType == BattleTypeController.BattleType.RegularBattle)
        {
            if (deity.PerformDeityEnmityCheck())
            {
                Attack(deity);
            }
            else
            {
                BattleInterface.Instance.SetDeityNotification("Deity Anguana placidly looks around");
                Debug.Log("Deity Anguana doesn't do anything");
            }
        }
        else if (BattleTypeController.Instance.currentBattleType == BattleTypeController.BattleType.BattleWithDeity)
        {
            Attack(deity);
        }
    }
    public void Attack(Deity deity)
    {
        deity.deityCry.Play();
        BattleInterface.Instance.SetSpellNameOnNotificationPanel(attackName, deityName);
        GameObject[] playerUnitsOnBattlefield = GameObject.FindGameObjectWithTag("PlayerPartyController").GetComponent<PlayerPartyController>().playerUnitsOnBattlefield;
        foreach (var playerUnit in playerUnitsOnBattlefield)
        {
            GameObject newDeityAttackVFX = Instantiate(deity.deityAttackVFX, playerUnit.GetComponent<Unit>().ownedTile.transform.position, Quaternion.identity);
            Vector3 attackVFXOffset = new Vector3(0, 1, 0);
            newDeityAttackVFX.transform.localPosition += attackVFXOffset;
            Destroy(newDeityAttackVFX, vfxDurationDelay);
            playerUnit.GetComponent<Unit>().TakeDamage(deity.deitySpecialAttackPower);
        }
        deity.enmity = 0;
        deity.deityEnmityTracker.GetComponent<DeityEnmityTrackerController>().UpdateDeityEnmityTracker();
        Debug.Log("Anguana Executes Spiteful Wave attack");
    }
}