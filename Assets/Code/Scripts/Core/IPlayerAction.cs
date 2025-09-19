using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Edelweiss.Core

{
    public interface IPlayerAction<T>
    {
        //public void Select(TileController selectedTile);
        public void Execute(T target);
        //public void Deselect();

    }

}
