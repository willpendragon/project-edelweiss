using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionKey
{
    public int indexTileXPosition;
    public int indexTileYPosition;
    public GameObject tileController;

    public PositionKey(int tileXPosition, int tileYPosition, GameObject tileController)
    {
        this.indexTileXPosition = tileXPosition;
        this.indexTileYPosition = tileYPosition;
        this.tileController = tileController;
    }

    public override int GetHashCode()
    {
        return indexTileXPosition.GetHashCode() ^ indexTileYPosition.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        return obj is PositionKey key && indexTileXPosition == key.indexTileXPosition && indexTileYPosition == key.indexTileYPosition;
    }
}
