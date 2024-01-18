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
    [Header("Player Statistics")]
    public float attackPower;
    public float meleeAttackPower;
    public float healthPoints;
    public float manaPoints;
    public int attackModifier;
    public float unitAmorRating;
    public float unitShield;
    public float coins;
    public float playerExperiencePoints;

    [Header("Deity System Parameters")]
    public attackAlignmentType currentAttackAlignmentType;
    public Deity deity;
    public fieldEffect currentFieldEffect;

    [Header("UI")]
    public TextMeshProUGUI playerHealthPointsDisplay;
    public TextMeshProUGUI playerShieldDisplay;
    public TextMeshProUGUI playerManaPointsDisplay;

    [Header("Gameplay Logic")]
    //public Enemy enemyTarget;
    public BattleManager battleManager;
    public GameObject currentTarget;
    public TileController unitCurrentTile;

    [Header("Presentation")]
    public GameObject currentPosition;
    public Animator playerAnimator;

    public void OnEnable()
    {
        Deity.OnDeityFieldEffectActivation += SetPlayerCurrentFieldEffectStatus;
        Deity.OnDeityFieldEffectActivation += PlayHurtAnimation;
    }
    public void OnDisable()
    {
        Deity.OnDeityFieldEffectActivation -= SetPlayerCurrentFieldEffectStatus;
        Deity.OnDeityFieldEffectActivation -= PlayHurtAnimation;
    }
    public void UpdatePlayerHealthDisplay()
    {
        playerHealthPointsDisplay.text = healthPoints.ToString();
    }
    public void UpdatePlayerShieldDisplay()
    {
        playerShieldDisplay.text = unitShield.ToString();
    }
    public void UpdateManaPointsDisplay()
    {
        playerManaPointsDisplay.text = manaPoints.ToString();
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
        unitCurrentTile = currentTile;
    }
    public void SetPlayerCurrentFieldEffectStatus()
    {
        currentFieldEffect = fieldEffect.iceMist;
    }
}