using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TriadAttacksController : MonoBehaviour
{
    private int triadAttackLimiter = 1;
    [SerializeField] float triadDamage;
    [SerializeField] Button triadAttackButton;
    private void OnEnable()
    {
        MovePlayerAction.OnUnitMovedToTile += CheckTriadLink;
    }
    private void OnDisable()
    {
        MovePlayerAction.OnUnitMovedToTile -= CheckTriadLink;
    }
    private void Start()
    {
        if (triadAttackButton != null)
        {
            triadAttackButton.interactable = false;
        }
    }
    private void CheckTriadLink(TileController destinationTile)
    {
        Debug.Log("Unit walked on a Tile");
        if (destinationTile != null && CheckTileTriadProperty(destinationTile))
        {
            Debug.Log("Unit walked on a Triad Tile");
            MakeTriadAttackButtonInteractable();
        }
    }
    private bool CheckTileTriadProperty(TileController destinationTile)
    {
        if (destinationTile != null && destinationTile.tileType == TileType.Triad)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    private void MakeTriadAttackButtonInteractable()
    {
        // Activate Triad Button Attack on the UI.
        if (triadAttackLimiter > 0 && triadAttackButton != null)
        {
            triadAttackButton.interactable = true;
            Debug.Log("Activated Triad Attack Button");
        }
    }
    public void ExecuteTriadAttack()
    {
        foreach (var enemyUnitGO in BattleManager.Instance.enemiesOnBattlefield)
        {
            Unit enemyUnit = enemyUnitGO.GetComponent<Unit>();
            if (enemyUnit.currentUnitLifeCondition != Unit.UnitLifeCondition.unitDead)
            {
                enemyUnit.TakeDamage(triadDamage);
                // Destroy Triad Attack button.
            }
        }
        triadAttackLimiter = 0;
    }
}