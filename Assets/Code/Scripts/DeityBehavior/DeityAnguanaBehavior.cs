using UnityEngine;

[CreateAssetMenu(fileName = "AnguanaBehavior", menuName = "DeityBehavior/Anguana")]
public class DeityAnguanaBehavior : DeityBehavior
{
    public int deityPrayerPowerMinimumRequirement;
    public override void ExecuteBehavior(Deity deity)
    {
        if (deity.deityPrayerPower > deityPrayerPowerMinimumRequirement)
        {
            Debug.Log("Attacking Enemies");
        }
        else if (deity.PerformDeityEnmityCheck())
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
            //07022024 Will need to add the damage taking logic for the Player Units
            // Anguana's specific behavior implementation goes here.
            // For example, checking the enmity meter and unleashing an attack.
        }
        else
        {
            Debug.Log("Deity Anguana doesn't do anything");
        }

    }
}