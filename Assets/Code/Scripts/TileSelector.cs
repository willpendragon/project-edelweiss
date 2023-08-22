using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileSelector : MonoBehaviour
{
    public enum TileCursorActivationStatus
    {
        tileCursorIsActive,
        tileCursorIsNotActive,
    }
    public enum TileCursorStatus
    {
        tileIsSelected,
        tileIsDeselected
    }
    [SerializeField] SpriteRenderer tileCursor;
    [SerializeField] TileCursorStatus currentTileCursorStatus;
    [SerializeField] TileCursorActivationStatus currentTileCursorActivationStatus;
    public Moveset moveset;
    // Start is called before the first frame update
    void Start()
    {
        currentTileCursorActivationStatus = TileCursorActivationStatus.tileCursorIsNotActive;
        moveset = GameObject.FindGameObjectWithTag("Player").GetComponent<Moveset>();
    }

    private void OnEnable()
    {
        Moveset.OnPlayerChangesPosition += ActivateTileCursor;
        Moveset.OnPlayerMovementModeEnd += DeactivateTileCursor;
    }
    private void OnDisable()
    {
        Moveset.OnPlayerChangesPosition -= ActivateTileCursor;
        Moveset.OnPlayerMovementModeEnd -= DeactivateTileCursor;

    }
    void ActivateTileCursor()
    {
        currentTileCursorActivationStatus = TileCursorActivationStatus.tileCursorIsActive;
    }

    void DeactivateTileCursor()
    {
        currentTileCursorActivationStatus = TileCursorActivationStatus.tileCursorIsNotActive;
    }

    void OnMouseOver()
    {
        if (currentTileCursorStatus != TileCursorStatus.tileIsSelected && currentTileCursorActivationStatus == TileCursorActivationStatus.tileCursorIsActive)
        {
            tileCursor.color = new Color (0,0,0,1);
        }
    }
    void OnMouseDown()
    {
        if (currentTileCursorActivationStatus == TileCursorActivationStatus.tileCursorIsActive)
        {
            tileCursor.color = Color.green;
            currentTileCursorStatus = TileCursorStatus.tileIsSelected;
            moveset.SelectCurrentPosition(this.gameObject);
            Debug.Log(this.gameObject);
        }
    }

    void OnMouseExit()
    {
        tileCursor.color = new Color (0,0,0,0);
        currentTileCursorStatus = TileCursorStatus.tileIsDeselected;
    }
}
