using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapManager2D : MonoBehaviour
{
    private static MapManager _instance;
    public static MapManager Instance { get { return _instance; } }

    public Tilemap tilemap;
    public GameObject overlayTile;
    public List<Vector3> availablePlaces;
    [SerializeField] Vector3 overlayTileOffset;

    void Start()
    {
        RetrievePlacesLocation();
    }

    void RetrievePlacesLocation()
    {
        
        availablePlaces = new List<Vector3>();
        for (int n = tilemap.cellBounds.xMin; n < tilemap.cellBounds.xMax; n++) //Scan tiles from Left to Right
        {
            for (int p = tilemap.cellBounds.yMin; p < tilemap.cellBounds.yMax; p++) // Scan tiles from Top to Bottom
            {
                Vector3Int localPlace = new Vector3Int(n, p, (int)tilemap.transform.position.y);
                Vector3 place = tilemap.CellToWorld(localPlace);
                if (tilemap.HasTile(localPlace))
                {
                    availablePlaces.Add(place);
                    Debug.Log("Adding tile to available places list");
                }
                else
                {
                    Debug.Log("No tile in place");
                } 
            }
        }
    }

    void SpawnTileOverlay()
    {

    }

}