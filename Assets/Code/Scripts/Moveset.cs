using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Moveset : Player
{
    [SerializeField] BattleInterface battleInterface;
    [SerializeField] Player player;
    [SerializeField] TileSelector tileSelector;
    [SerializeField] GameObject playerModel;

    public delegate void PlayerTurnIsOver();
    public static event PlayerTurnIsOver OnPlayerTurnIsOver;

    public delegate void PlayerChangesPosition();
    public static event PlayerChangesPosition OnPlayerChangesPosition;

    public delegate void PlayerMovementModeEnd();
    public static event PlayerMovementModeEnd OnPlayerMovementModeEnd;

    public delegate void PlayerMeleeAttack(Transform enemyTargetTransform);
    public static event PlayerMeleeAttack OnPlayerMeleeAttack;
    public void SelectCurrentTarget(GameObject selectedCurrentTarget)
    {
        currentTarget = selectedCurrentTarget;
    }
    public void SelectCurrentPosition(GameObject selectedCurrentTile)
    {
        if (selectedCurrentTile.GetComponent<TileController>().currentTileStatus == TileController.TileStatus.occupied)
        {
            Debug.Log("Tile occupied. Can't move the character here.");
        }
        else
        {
            currentPosition = selectedCurrentTile;
            selectedCurrentTile.GetComponent<TileController>().detectedUnit = this.gameObject;
            //this.gameObject.transform.position = currentPosition.transform.position;
            //Possible solution: scan all of the other tiles and remove from them the Player.
            player.transform.position = currentPosition.transform.position;
        }
    }
    public void RedAttack()
    {
        if (battleManager.currentTurnOrder == TurnOrder.playerTurn && currentTarget != null)
        {
            if (manaPoints > 0)
            {
                battleInterface.SetMovePanelName("Red Attack");
                currentAttackAlignmentType = attackAlignmentType.red;
                //CalculateModifier();
                currentTarget.GetComponent<Enemy>().TakeDamage(attackPower);
                currentTarget.GetComponent<Enemy>().UpdateEnemyHealthDisplay();
                deity.SinTracker(currentAttackAlignmentType, this.gameObject);
                OnPlayerTurnIsOver();
                OnPlayerMeleeAttack(currentTarget.transform);
                player.gameObject.GetComponentInChildren<Moveset>().manaPoints -= 10;
                UpdateManaPointsDisplay();
                Debug.Log("Red attack");
            }
            else
            {
                Debug.Log("Not enough Mana to perform the Red Attack");
            }
        }
        else
            Debug.Log("Not able to attack");
    }

    public void BlueAttack()
    {
        if (battleManager.currentTurnOrder == TurnOrder.playerTurn && currentTarget != null)
        {
            if (manaPoints > 0)
            {
                battleInterface.SetMovePanelName("Blue Attack");
                currentAttackAlignmentType = attackAlignmentType.blue;
                currentTarget.GetComponent<Enemy>().TakeDamage(attackPower + 10);
                currentTarget.GetComponent<Enemy>().UpdateEnemyHealthDisplay();
                deity.SinTracker(currentAttackAlignmentType, this.gameObject);
                OnPlayerTurnIsOver();
                player.gameObject.GetComponentInChildren<Moveset>().manaPoints -= 5;
                UpdateManaPointsDisplay();
                Debug.Log("Blue attack");
            }
            else
            {
                Debug.Log("Not enough Mana to perform the Blue Attack");
            }
        }
        else
            Debug.Log("Not able to attack");
    }

    public void Protection()
    {
        battleInterface.SetMovePanelName("Protection");
        if (battleManager.currentTurnOrder == TurnOrder.playerTurn)
        {
            shield += 10;
            UpdatePlayerShieldDisplay();
            currentAttackAlignmentType = attackAlignmentType.blue;
            deity.SinTracker(currentAttackAlignmentType, this.gameObject);
            OnPlayerTurnIsOver();
            Debug.Log("Shield Increased");
        }
        else
            Debug.Log("Not able to execute move");
    }

    public void ChangePosition()
    {
        if (this.gameObject.GetComponent<Player>().currentFieldEffect == fieldEffect.noFieldEffect && this.player.GetComponent<UnitStatusController>().unitCurrentStatus == UnitStatus.basic)
        {
            OnPlayerChangesPosition();
        }
        if (this.gameObject.GetComponent<Player>().currentFieldEffect == fieldEffect.noFieldEffect && this.player.GetComponent<UnitStatusController>().unitCurrentStatus == UnitStatus.stun)
        {
            Debug.Log("Unable to change position");
        }
        if (this.gameObject.GetComponent<Player>().currentFieldEffect == fieldEffect.iceMist)
        {
            int changePositionChance = Random.Range(0, 3);
            if (changePositionChance >= 2)
            {
                OnPlayerChangesPosition();
            }
            else
            {
                OnPlayerMovementModeEnd();
                Debug.Log("Can't move the Player on the battlefield");
            }
        }
    }

    public void EndMovementMode()
    {
        OnPlayerMovementModeEnd();
    }
}
