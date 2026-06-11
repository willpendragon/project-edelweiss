using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using System;
using ProjectEdelweiss.Utils;

[CreateAssetMenu(fileName = "AnguanaBehavior", menuName = "DeityBehavior/Anguana")]
public class DeityAnguanaBehavior : DeityBehavior
{
    public float vfxDurationDelay = 1f;
    private string deityName = "Anguana";
    public string attackName;

    private System.Random localRandom;

    public override void ExecuteBehavior(Deity deity)
    {
        if (deity.currentDeityStatus == Deity.DeityStatus.Summoned)
            return;

        BattleManager battleManager = GameObject.FindGameObjectWithTag("BattleManager").GetComponent<BattleManager>();

        if (BattleTypeController.Instance.currentBattleType == BattleTypeController.BattleType.RegularBattle)
        {
            // Attack Routine
            AttemptAttack(deity);
        }
        else if (BattleTypeController.Instance.currentBattleType == BattleTypeController.BattleType.BattleWithDeity)
        {
            AttemptAttack(deity);
            DOVirtual.DelayedCall(1.5f, () => MoveObelisk(deity));
        }
    }

    public override void ExecuteBuffBehaviour(Deity deity, Unit unit)
    {
        //
    }

    private void MoveObelisk(Deity deity)
    {
        // Logic.
        MoveDeityToRandomTile(deity);
        GameObject deitySpawnerGameObject = GameObject.FindGameObjectWithTag("DeitySpawner");
        // Only the Obelisk conduit moves on the Battlefield.
        DeitySpawner deitySpawner = deitySpawnerGameObject.GetComponent<DeitySpawner>();
        // Physically move the Obelisk.        
        deitySpawner.MoveObeliskOnGridMap();
        DOVirtual.DelayedCall(1f,
            () => BattleInterface.Instance.SetDeityNotification($"Deity {deityName} moved its Altar."));
    }

    private void AttemptAttack(Deity deity)
    {
        if (deity.PerformDeityEnmityCheck())
        {
            DOVirtual.DelayedCall(1.5f, () => Attack(deity));
        }
        else
        {
            DOVirtual.DelayedCall(1f,
                () => BattleInterface.Instance.SetDeityNotification($"Deity {deityName} placidly looks around"));
        }
    }

    public void Attack(Deity deity)
    {
        BattleInterface.Instance.SetDeityNotification($"Deity {deityName} used {attackName}");
        deity.deityCry.Play();

        GameObject[] playerUnitsOnBattlefield = GameObject.FindGameObjectWithTag("PlayerPartyController")
            .GetComponent<PlayerPartyController>().playerUnitsOnBattlefield;

        foreach (var playerUnit in playerUnitsOnBattlefield)
        {
            GameObject newDeityAttackVFX = Instantiate(deity.deityAttackVFX,
                playerUnit.GetComponent<Unit>().ownedTile.transform.position, Quaternion.identity);
            Vector3 attackVFXOffset = new Vector3(0, 1, 0);
            newDeityAttackVFX.transform.localPosition += attackVFXOffset;
            Destroy(newDeityAttackVFX, vfxDurationDelay);
            playerUnit.GetComponent<Unit>().TakeDamage(deity.deitySpecialAttackPower);
        }

        // Reset Anguana's enmity.
        ResetEnmityWrapper();
    }

    private void ResetEnmityWrapper()
    {
        var deityReference = BattleManager.Instance.enemyTurnManager.deity.GetComponent<Deity>();
        deityReference.ResetDeityEnmity();
    }
}