using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionKey
{
    public int indexTileXPosition; // Mappata su X
    public int indexTileYPosition; // Elevazione (nuova Y del Vector3Int)
    public int indexTileZPosition; // Profondità (vecchia Y 2D, ora Z del Vector3Int)
    public GameObject tileController;

    public PositionKey(int tileXPosition, int tileYPosition, int tileZPosition, GameObject tileController)
    {
        this.indexTileXPosition = tileXPosition;
        this.indexTileYPosition = tileYPosition;
        this.indexTileZPosition = tileZPosition;
        this.tileController = tileController;
    }

    // Costruttore di comodità per un Vector3Int
    public PositionKey(Vector3Int gridPosition, GameObject tileController = null)
    {
        this.indexTileXPosition = gridPosition.x;
        this.indexTileYPosition = gridPosition.y;
        this.indexTileZPosition = gridPosition.z;
        this.tileController = tileController;
    }

    public override int GetHashCode()
    {
        // Hash combinato di tutte e tre le dimensioni
        return indexTileXPosition.GetHashCode() ^ indexTileYPosition.GetHashCode() ^ indexTileZPosition.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        return obj is PositionKey key && 
               indexTileXPosition == key.indexTileXPosition && 
               indexTileYPosition == key.indexTileYPosition &&
               indexTileZPosition == key.indexTileZPosition;
    }
}
