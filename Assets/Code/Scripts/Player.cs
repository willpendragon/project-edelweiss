using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;

public enum attackAlignmentType
{
    red,
    blue
}

public enum fieldEffect
{
    noFieldEffect,
    iceMist

}


public class Player : MonoBehaviour

{
    public Enemy enemyTarget;
    public float attackPower;
    public float healthPoints;
    public attackAlignmentType currentAttackAlignmentType;
    public TextMeshProUGUI playerHealthPointsDisplay;
    public TextMeshProUGUI playerShieldDisplay;    
    public GameObject currentTarget;
    public GameObject currentPosition;
    public BattleManager battleManager;
    public Deity deity;
    public fieldEffect currentFieldEffect;
    public int attackModifier;
    public float shield;
    public TileController unitCurrentTile;
    [SerializeField] Animator playerAnimator;

    public void Start()
    {
        UpdatePlayerHealthDisplay();
        UpdatePlayerShieldDisplay();
    }

    public void SwitchToEnemyTurn()
    {
        battleManager.PassTurnToEnemies(); 
    }

    public void UpdatePlayerHealthDisplay()
    {
        playerHealthPointsDisplay.text = healthPoints.ToString();
    }
    public void UpdatePlayerShieldDisplay()
    {
        playerShieldDisplay.text = shield.ToString();
    }

    public void PlayHurtAnimation()
    {
        playerAnimator.SetBool("Hurt", true);
        StartCoroutine("ResetPlayerHurtAnimation");
    }

    IEnumerator ResetPlayerHurtAnimation()
    {
        yield return new WaitForSeconds(1);
        playerAnimator.SetBool("Hurt", false);
    }

    public void SetUnitCurrentTile(TileController currentTile)
    {
        currentTile = unitCurrentTile;
    }
}