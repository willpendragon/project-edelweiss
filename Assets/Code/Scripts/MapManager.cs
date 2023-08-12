using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour
{
    private static MapManager _instance;
    public static MapManager Instance { get { return _instance; } }

    public Tilemap tilemap;
    public GameObject overlayTile;
    [SerializeField] Vector3 overlayTileOffset;

    void Start()
    {
        SpawnTilesOverlay();
    }

    void SpawnTilesOverlay()
    {
        foreach (Transform child in tilemap.transform)
        {
            Instantiate(overlayTile, child.position + overlayTileOffset, Quaternion.Euler(90, 0, 0));
            Debug.Log("Found Tile");
        }
    }
}