using UnityEngine;

[CreateAssetMenu(fileName = "KingLaurinusBehavior", menuName = "DeityBehavior/KingLaurinus")]
public class DeityKingLaurinusBehavior : DeityBehavior
{
    public override void ExecuteBehavior(Deity deity)
    {
        GameObject[] playerUnitsOnBattlefield = GameObject.FindGameObjectWithTag("PlayerPartyController").GetComponent<PlayerPartyController>().playerUnitsOnBattlefield;
        foreach (var playerUnit in playerUnitsOnBattlefield)
        {
            playerUnit.GetComponent<Unit>().unitHealthPoints -= deity.deitySpecialAttackPower;
        }
        Debug.Log("King Laurinus Executes Cursed Garden attack");
        // King Laurinus' specific behavior implementation goes here.
        // For example, checking the enmity meter and unleashing an attack.
    }
}