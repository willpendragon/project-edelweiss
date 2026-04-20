using UnityEngine;

public class Gate : MonoBehaviour
{
    public int linkID = 0; // Set this in the Map Editor / Prefab

    public int tileX;
    public int tileY; // Acting as Z in your 3D grid
    
    private TileController _myTile;
    private bool _isOpen = false;

    // Optional: Reference to the visual mesh to hide/show it
    [SerializeField] private GameObject gateMesh; 

    private void Start()
    {
        SyncWithGrid();
        
        // By default, gates start closed and block the tile
        CloseGate(); 
    }

    public void SyncWithGrid()
    {
        Vector2Int coords = GridManager.Instance.GetGridCoordinatesFromWorldPosition(transform.position);
        tileX = coords.x;
        tileY = coords.y;

        _myTile = GridManager.Instance.GetTileControllerInstance(tileX, tileY);
    }

    public void OpenGate()
    {
        if (_isOpen) return;
        _isOpen = true;

        if (_myTile != null)
        {
            _myTile.currentSingleTileCondition = SingleTileCondition.free;
            
            // Clear the detected unit so the tile routing knows it's empty
            if (_myTile.detectedUnit == this.gameObject)
            {
                _myTile.detectedUnit = null;
            }
        }

        // Hide the physical gate
        if (gateMesh != null) gateMesh.SetActive(false);
        else gameObject.GetComponentInChildren<Renderer>().enabled = false;

        Debug.Log($"Gate {linkID} Opened! Tile is now walkable.");
    }

    public void CloseGate()
    {
        if (!_isOpen && Time.time > 1f) return; // Prevent spamming on Start
        _isOpen = false;

        if (_myTile != null)
        {
            // Only close if there isn't a unit currently standing inside the open gate
            if (_myTile.detectedUnit == null || _myTile.detectedUnit == this.gameObject)
            {
                _myTile.currentSingleTileCondition = SingleTileCondition.occupied;
                _myTile.detectedUnit = this.gameObject;
            }
            else
            {
                Debug.LogWarning($"Gate {linkID} tried to close, but a unit was standing in it!");
                // You can add logic here to crush the player or wait until they move
            }
        }

        // Show the physical gate
        if (gateMesh != null) gateMesh.SetActive(true);
        else gameObject.GetComponentInChildren<Renderer>().enabled = true;

        Debug.Log($"Gate {linkID} Closed! Tile is now blocked.");
    }
}
