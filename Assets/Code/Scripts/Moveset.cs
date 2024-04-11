//using System.Collections;
//using System.Collections.Generic;
//using Unity.VisualScripting;
//using UnityEngine;
//using UnityEngine.Events;

//public class Moveset : MonoBehaviour
//{
//    [SerializeField] BattleInterface battleInterface;
//    public Player player;
//    [SerializeField] TileSelector tileSelector;
//    [SerializeField] GameObject playerModel;
//    [SerializeField] int opportunityPoints;
//    [SerializeField] int maximumOpportunityPoints;
//    [SerializeField] float endPlayerTurnDelay;
//    [SerializeField] float protectionShieldIncreaseValue;

//    public delegate void PlayerTurnIsOver();
//    public static event PlayerTurnIsOver OnPlayerTurnIsOver;

//    public delegate void PlayerChangesPosition();
//    public static event PlayerChangesPosition OnPlayerChangesPosition;

//    public delegate void PlayerMovementModeEnd();
//    public static event PlayerMovementModeEnd OnPlayerMovementModeEnd;

//    public delegate void PlayerMeleeAttack(Transform enemyTargetTransform);
//    public static event PlayerMeleeAttack OnPlayerMeleeAttack;

//    public UnityEvent CastingSpell;
//    public void SelectCurrentTarget(GameObject selectedCurrentTarget)
//    {
//        player.currentTarget = selectedCurrentTarget;
//    }
//    public void SelectCurrentPosition(GameObject selectedCurrentTile)
//    {
//        /*if (selectedCurrentTile.GetComponent<TileController>().currentTileStatus == TileController.TileStatus.occupied)
//        {
//            Debug.Log("Tile occupied. Can't move the character here.");
//        }
//        else
//        {
//            player.currentPosition = selectedCurrentTile;
//            selectedCurrentTile.GetComponent<TileController>().detectedUnit = this.gameObject;
//            player.transform.position = player.currentPosition.transform.position;
//        }
//        */
//    }
//    public void MeleeAttack()
//    {
//        Debug.Log("Melee Attack");
//        if (player.currentFieldEffect == fieldEffect.iceMist)
//        {
//            int meleeAttackChance = Random.Range(0, 3);
//            if (meleeAttackChance >= 2)
//            {
//                Debug.Log("This unit is unable to perform a Melee Attack");
//            }
//        }
//        else if (player.currentFieldEffect == fieldEffect.noFieldEffect)
//        {
//            PerformMeleeAttack();
//        }
//    }
//    public void PerformMeleeAttack()
//    {
//        //battleInterface.SetMovePanelName("Melee Attack");
//        //player.currentTarget.GetComponent<EnemyAgent>().TakeDamage(player.meleeAttackPower);
//        opportunityPoints--;
//        CheckOpportunityPoints();
//        StartCoroutine("SendMeleeAttackFeedback");
//    }
//    IEnumerator SendMeleeAttackFeedback()
//    {
//        yield return new WaitForSeconds(0.1f);
//        OnPlayerMeleeAttack(player.currentTarget.transform);
//    }

//    public void RedAttack()
//    {
//        if (player.battleManager.currentTurnOrder == TurnOrder.playerTurn && player.currentTarget != null)
//        {
//            if (player.manaPoints > 0)
//            {
//                //battleInterface.SetMovePanelName("Red Attack");
//                player.currentAttackAlignmentType = attackAlignmentType.red;
//                //player.currentTarget.GetComponent<EnemyAgent>().TakeDamage(player.attackPower);
//                //currentTarget.GetComponent<Enemy>().UpdateEnemyHealthDisplay();
//                //player.deity.SinTracker(player.currentAttackAlignmentType, this.gameObject);
//                opportunityPoints--;
//                OnPlayerMeleeAttack(player.currentTarget.transform);
//                player.gameObject.GetComponentInChildren<Moveset>().player.manaPoints -= 10;
//                player.UpdateManaPointsDisplay();
//                CheckOpportunityPoints();
//                Debug.Log("Red attack");
//            }
//            else
//            {
//                Debug.Log("Not enough Mana to perform the Red Attack");
//            }
//        }
//        else
//            Debug.Log("Not able to attack");
//    }

//    public void BlueAttack()
//    {
//        if (player.battleManager.currentTurnOrder == TurnOrder.playerTurn && player.currentTarget != null)
//        {
//            if (player.manaPoints > 0)
//            {
//                //battleInterface.SetMovePanelName("Blue Attack");
//                player.currentAttackAlignmentType = attackAlignmentType.blue;
//                //player.currentTarget.GetComponent<EnemyAgent>().TakeDamage(player.attackPower + 10);
//                //player.deity.SinTracker(player.currentAttackAlignmentType, this.gameObject);
//                opportunityPoints--;
//                player.gameObject.GetComponentInChildren<Moveset>().player.manaPoints -= 5;
//                player.UpdateManaPointsDisplay();
//                CastingSpell.Invoke();
//                CheckOpportunityPoints();
//                Debug.Log("Blue attack");
//            }
//            else
//            {
//                Debug.Log("Not enough Mana to perform the Blue Attack");
//            }
//        }
//        else
//            Debug.Log("Not able to attack");
//    }

//    public void Protection()
//    {
//        //battleInterface.SetMovePanelName("Protection");
//        if (player.battleManager.currentTurnOrder == TurnOrder.playerTurn)
//        {
//            player.unitShield += protectionShieldIncreaseValue;
//            player.UpdatePlayerShieldDisplay();
//            player.currentAttackAlignmentType = attackAlignmentType.blue;
//            //player.deity.SinTracker(player.currentAttackAlignmentType, this.gameObject);
//            opportunityPoints--;
//            CheckOpportunityPoints();
//            Debug.Log("Shield Increased");
//        }
//        else
//            Debug.Log("Not able to execute move");
//    }

//    public void ChangePosition()
//    {
//        if (this.gameObject.GetComponent<Player>().currentFieldEffect == fieldEffect.noFieldEffect && this.player.GetComponent<UnitStatusController>().unitCurrentStatus == UnitStatus.basic)
//        {
//            OnPlayerChangesPosition();
//            opportunityPoints--;
//        }
//        if (this.gameObject.GetComponent<Player>().currentFieldEffect == fieldEffect.noFieldEffect && this.player.GetComponent<UnitStatusController>().unitCurrentStatus == UnitStatus.stun)
//        {
//            Debug.Log("Unable to change position");
//        }
//        if (this.gameObject.GetComponent<Player>().currentFieldEffect == fieldEffect.iceMist)
//        {
//            int changePositionChance = Random.Range(0, 3);
//            if (changePositionChance >= 2)
//            {
//                OnPlayerChangesPosition();
//                opportunityPoints--;
//            }
//            else
//            {
//                OnPlayerMovementModeEnd();
//                Debug.Log("Can't move the Player on the battlefield");
//            }
//        }
//    }

//    public void CheckOpportunityPoints()
//    {
//        if (opportunityPoints == 0)
//        {
//            StartCoroutine(EndPlayerTurn());
//        }
//    }
//    IEnumerator EndPlayerTurn()
//    {
//        yield return new WaitForSeconds(endPlayerTurnDelay);
//        OnPlayerTurnIsOver();
//    }
//    public void EndMovementMode()
//    {
//        OnPlayerMovementModeEnd();
//    }
//    public void RestoreOpportunityPoints()
//    {
//        opportunityPoints = maximumOpportunityPoints;
//    }
//}
