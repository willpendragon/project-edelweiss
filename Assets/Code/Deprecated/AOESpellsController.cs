using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AOESpellsController : MonoBehaviour
{
    public GridMovementController gridMovementController;
    public TileController tile;
    // Start is called before the first frame update
    public void SelectMultipleTiles()
    {
        foreach (var tile in gridMovementController.GetMultipleTiles(tile))
        {
            tile.GetComponentInChildren<MeshRenderer>().material.color = Color.blue;
            Destroy(tile.detectedUnit);
        }
    }
}
