using UnityEngine;

[CreateAssetMenu(fileName = "AnguanaBehavior", menuName = "DeityBehavior/Anguana")]
public class DeityAnguanaBehavior : DeityBehavior
{
    public override void ExecuteBehavior(Deity deity)
    {
        GameObject[] playerUnitsOnBattlefield = GameObject.FindGameObjectWithTag("PlayerPartyController").GetComponent<PlayerPartyController>().playerUnitsOnBattlefield;
        foreach (var playerUnit in playerUnitsOnBattlefield)
        {
            playerUnit.GetComponent<Unit>().unitHealthPoints -= deity.deitySpecialAttackPower;
        }
        Debug.Log("Anguana Executes Spiteful Wave attack");
        // Anguana's specific behavior implementation goes here.
        // For example, checking the enmity meter and unleashing an attack.
    }
}