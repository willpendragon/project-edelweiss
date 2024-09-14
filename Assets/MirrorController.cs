using System.Collections.Generic;
using UnityEngine;

public class MirrorController : MonoBehaviour
{
    [SerializeField] GridManager gridManager;
    [SerializeField] BossController bossController;
    [SerializeField] GameObject mirrorGO;
    [SerializeField] GameObject activationPlatformGO;

    List<TileController> activationPlatforms = new List<TileController>();

    private int activatedPlatformsCount;

    public delegate void MirrorAttack(string mirrorAttackNotification);
    public static event MirrorAttack OnMirrorAttack;

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
        SpawnMirrors();
    }
    private void PopulateActivationPlatformList()
    {
        foreach (var tile in gridManager?.gridTileControllers)
        {
            if (tile != null && tile.tileType == TileType.ActivationPlatform)
            {
                activationPlatforms.Add(tile);
                ChangeTileColor(tile);
                if (activationPlatformGO != null)
                {
                    float activationPlatformHeight = 1f;
                    GameObject activationPlatformGOInstance = Instantiate(activationPlatformGO, tile.gameObject.transform.position, Quaternion.identity);
                    activationPlatformGOInstance.transform.position = new Vector3(tile.transform.position.x, activationPlatformHeight, tile.transform.position.z);
                }
                Debug.Log(activationPlatforms);
            }
            else
            {
                Debug.Log("No Activation Platform Tile found");
            }
        }
    }

    private void SpawnMirrors()
    {
        foreach (var tile in gridManager?.gridTileControllers)
        {
            if (tile != null && tile.tileType == TileType.Mirror)
            {
                if (mirrorGO != null)
                {
                    float mirrorHeight = 1.45f;
                    GameObject mirrorGOInstance = Instantiate(mirrorGO, tile.transform.position, Quaternion.identity);
                    mirrorGOInstance.transform.position = new Vector3(tile.transform.position.x, mirrorHeight, tile.transform.position.z);
                }
                OccupyTile(tile);
            }
            else
            {
                Debug.Log("No Mirror Tile found");
            }
        }
    }

    private void OccupyTile(TileController tileToOccupy)
    {
        tileToOccupy.currentSingleTileCondition = SingleTileCondition.occupied;
        Debug.Log(tileToOccupy + " set to" + tileToOccupy.currentSingleTileCondition);
    }

    private void ChangeTileColor(TileController mirrorTile)
    {
        SpriteRenderer tileSpriteRenderer = mirrorTile.gameObject.GetComponentInChildren<SpriteRenderer>();
        if (tileSpriteRenderer != null)
        {
            tileSpriteRenderer.color = Color.yellow;
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
            else
            {
                Debug.Log("No Player Unit found on the Mirror Activation Platforms");
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
        int mirrorDamage = 500;
        OnMirrorAttack("The Mirrors Hit the Boss");
        mirrorTarget.TakeDamage(mirrorDamage);
        //Insert trigger for Mirror VFX here

        Debug.Log("Used Mirror Attack on Boss Unit");
    }
}
