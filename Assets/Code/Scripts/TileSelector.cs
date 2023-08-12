using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileSelector : MonoBehaviour
{
    public enum TileCursorStatus
    {
        tileIsSelected,
        tileIsDeselected
    }
    [SerializeField] SpriteRenderer tileCursor;
    [SerializeField] TileCursorStatus currentTileCursorStatus;
    public Moveset moveset;
    // Start is called before the first frame update
    void Start()
    {
        moveset = GameObject.FindGameObjectWithTag("Player").GetComponent<Moveset>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    void OnMouseOver()
    {
        if (currentTileCursorStatus != TileCursorStatus.tileIsSelected)
        {
            tileCursor.color = new Color (0,0,0,1);
        }
    }
    void OnMouseDown()
    {
        tileCursor.color = Color.green;
        currentTileCursorStatus = TileCursorStatus.tileIsSelected;
        moveset.SelectCurrentPosition(this.gameObject);
        Debug.Log(this.gameObject);
    }

    void OnMouseExit()
    {
        tileCursor.color = new Color (0,0,0,0);
        currentTileCursorStatus = TileCursorStatus.tileIsDeselected;
    }
}
