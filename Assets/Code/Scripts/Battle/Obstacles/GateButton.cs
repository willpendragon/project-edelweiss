using UnityEngine;

public class GateButton : MonoBehaviour
{
    public int linkID = 0; // Set this in the Map Editor / Prefab

    public int tileX;
    public int tileY; // Acting as Z in your 3D grid
    
    private TileController _myTile;
    private bool _isPressed = false;

    private Gate[] _connectedGates;

    private void Start()
    {
        SyncWithGrid();
        FindConnectedGates();
    }

    private void Update()
    {
        if (_myTile == null) return;

        // Check if there's a unit standing on our tile (and ensure it's not the button itself registering as the unit)
        bool hasUnit = (_myTile.detectedUnit != null && _myTile.detectedUnit != this.gameObject);

        // State changed to Pressed
        if (hasUnit && !_isPressed)
        {
            _isPressed = true;
            Debug.Log($"GateButton {linkID} Pressed!");
            
            foreach (var gate in _connectedGates)
            {
                gate.OpenGate();
            }
        }
        // State changed to Unpressed (Unit stepped off)
        else if (!hasUnit && _isPressed)
        {
            _isPressed = false;
            Debug.Log($"GateButton {linkID} Released!");
            
            foreach (var gate in _connectedGates)
            {
                gate.CloseGate();
            }
        }
    }

    public void SyncWithGrid()
    {
        Vector2Int coords = GridManager.Instance.GetGridCoordinatesFromWorldPosition(transform.position);
        tileX = coords.x;
        tileY = coords.y;

        _myTile = GridManager.Instance.GetTileControllerInstance(tileX, tileY);

        // Make sure the button itself doesn't block player movement!
        if (_myTile != null)
        {
            _myTile.currentSingleTileCondition = SingleTileCondition.free;
            // Optional: If the GridManager set this object as the detectedUnit during map generation, clear it so a player can step here
            if (_myTile.detectedUnit == this.gameObject) 
            {
                _myTile.detectedUnit = null; 
            }
        }
    }

    private void FindConnectedGates()
    {
        // Find all gates in the scene and filter by our linkID
        var allGates = FindObjectsOfType<Gate>();
        var matchingGates = new System.Collections.Generic.List<Gate>();

        foreach (var gate in allGates)
        {
            if (gate.linkID == this.linkID)
            {
                matchingGates.Add(gate);
            }
        }
        _connectedGates = matchingGates.ToArray();
    }
}
