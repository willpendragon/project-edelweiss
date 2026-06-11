using ProjectEdelweiss.Utils;
using UnityEngine;

public class DeityPowerController : MonoBehaviour
{
    public delegate void PlayerUnitPraying();

    public static event PlayerUnitPraying OnPlayerUnitPraying;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            UseDeityMove();
        }
    }

    public void UseDeityMove()
    {
        Unit activePlayerUnit = GameObject.FindGameObjectWithTag(GameTags.ActivePlayerUnit)?.GetComponent<Unit>();
        if (activePlayerUnit == null)
        {
            BattleInterface.Instance.SetBattleNotification("No Unit Selected!");
            return;
        }

        Deity deity = activePlayerUnit.linkedDeity;
        if (deity == null)
            return;

        // Retrieve the active unit's profile
        PlayerPartyProfileHelper profile = BattleInterface.Instance.PlayerPartyProfilesUIManager.RetrieveProfile(activePlayerUnit.unitTemplate.unitName);

        if (profile != null)
        {
            // Check if the cooldown is active
            if (!profile.IsDeityMoveReady())
            {
                BattleInterface.Instance.SetBattleNotification("Deity Move is still on cooldown!");
                return;
            }
        }

        deity.summoningBehaviour.ExecuteBehavior(deity);

        // Start the cooldown
        if (profile != null)
        {
            profile.StartCooldown();
        }
    }
}