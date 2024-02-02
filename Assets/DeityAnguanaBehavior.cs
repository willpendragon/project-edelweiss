using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

[CreateAssetMenu(fileName = "AnguanaBehavior", menuName = "DeityBehavior/Anguana")]
public class DeityAnguanaBehavior : DeityBehavior
{
    public override void ExecuteBehavior(Deity deity)
    {
        if (deity.PerformDeityEnmityCheck())
        {
            GameObject newDeityAttackVFX = Instantiate(deity.deityAttackVFX, deity.transform);
            Destroy(newDeityAttackVFX, 3);
            GameObject[] playerUnitsOnBattlefield = GameObject.FindGameObjectWithTag("PlayerPartyController").GetComponent<PlayerPartyController>().playerUnitsOnBattlefield;
            foreach (var playerUnit in playerUnitsOnBattlefield)
            {
                playerUnit.GetComponent<Unit>().unitHealthPoints -= deity.deitySpecialAttackPower;
            }
            deity.enmity = 0;
            deity.deityEnmityTracker.GetComponent<DeityEnmityTrackerController>().UpdateDeityEnmityTracker();
            Debug.Log("Anguana Executes Spiteful Wave attack");
            // Anguana's specific behavior implementation goes here.
            // For example, checking the enmity meter and unleashing an attack.
        }
        else
        {
            Debug.Log("Deity Anguana doesn't do anything");
        }

    }
}