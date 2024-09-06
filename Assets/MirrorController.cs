using System.Collections.Generic;
using UnityEngine;

public class MirrorController : MonoBehaviour
{
    [SerializeField] GridManager gridManager;
    [SerializeField] BossController bossController;
    List<TileController> activationPlatforms = new List<TileController>();
    private int activatedPlatformsCount;

    public void OnEnable()
    {
        EnemyTurnManager.OnPlayerTurn += ActivateMirrors;
    }
    public void OnDisable()
    {
        EnemyTurnManager.OnPlayerTurn -= ActivateMirrors;
    }

    public void Start()
    {
        PopulateActivationPlatformList();
    }
    private void PopulateActivationPlatformList()
    {
        if (gridManager != null)
        {
            foreach (var tile in gridManager.gridTileControllers)
            {
                if (tile != null && tile.tileType == TileType.ActivationPlatform)
                {
                    activationPlatforms.Add(tile);
                    Debug.Log(activationPlatforms);
                }
            }
        }
    }

    public void ActivateMirrors(string test)
    {
        // Added a "string" argument since the original delegate required it.
        // Need to create a new delegate without the parameters, specifically for the
        // Player turn handover.

        CheckActivationPlatformsStatus();
        if (CheckActivatedPlatformsRequisite())
        {
            UnleashMirrorAttack();
        }
    }

    private void CheckActivationPlatformsStatus()
    {
        foreach (var activationPlaform in activationPlatforms)
        {
            if (activationPlaform.detectedUnit != null)
            {
                if (activationPlaform.detectedUnit.tag == "Player")
                {
                    activatedPlatformsCount++;
                }
            }
        }
    }

    private bool CheckActivatedPlatformsRequisite()
    {
        int activatedPlatformsCountRequirement = 2;
        if (activatedPlatformsCount > activatedPlatformsCountRequirement)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    private void UnleashMirrorAttack()
    {
        // Here goes the logic for damaging the Boss Unit
        // Hard-coded for demo

        Unit mirrorTarget = bossController.bossUnit;
        int mirrorDamage = 200;
        mirrorTarget.HealthPoints -= mirrorDamage;
    }
}
