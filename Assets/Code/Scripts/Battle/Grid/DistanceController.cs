using UnityEngine;

public class DistanceController : MonoBehaviour
{
    // Rimuoviamo la dipendenza forzata da GridMovementController per i CheckLineari.

    public bool CheckDistance(TileController attackerTile, TileController defenderTile, int distanceThreshold = 1)
    {
        // 1. Safeguard against null target checks
        if (attackerTile == null || defenderTile == null)
        {
            Debug.LogWarning("Distance Check: Missing attacker or defender tile. Returning false.");
            return false;
        }

        int distance = GetManhattanDistanceVoxel(attackerTile, defenderTile);
        
        if (distance <= distanceThreshold)
        {
            Debug.Log("Distance Check: Attacker is close to Defender. Attack Modifier will apply");
            return true;
        }
        else
        {
            Debug.Log("Distance Check: Attacker is distant from Defender. Attack Modifier will NOT apply");
            Debug.Log($"Attacker coordinates {attackerTile.gridPosition.x}, {attackerTile.gridPosition.y}, {attackerTile.gridPosition.z}");
            Debug.Log($"Defender coordinates {defenderTile.gridPosition.x}, {defenderTile.gridPosition.y}, {defenderTile.gridPosition.z}");

            return false;
        }
    }

    public int GetManhattanDistanceVoxel(TileController tileA, TileController tileB)
    {
        if (tileA == null || tileB == null) return int.MaxValue;

        Vector3Int posA = tileA.gridPosition;
        Vector3Int posB = tileB.gridPosition;

        // Voxel Orthogonal Distance (Solitamente ignora elevazione Y se non conta come "costo" per l'attacco)
        return Mathf.Abs(posA.x - posB.x) + Mathf.Abs(posA.z - posB.z);
    }
}