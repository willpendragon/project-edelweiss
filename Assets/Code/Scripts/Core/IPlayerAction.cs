using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Edelweiss.Core

{
    public interface IPlayerAction<T>
    {
        public void Execute(T target);
    }
}
