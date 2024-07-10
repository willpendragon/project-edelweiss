using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerAction
{
    public void Select(TileController selectedTile);
    public void Execute();
    public void Deselect();

}
