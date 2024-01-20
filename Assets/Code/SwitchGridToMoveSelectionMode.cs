using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchGridToMoveSelectionMode : MonoBehaviour
{
    public delegate void MoveButtonPressed();
    public static event MoveButtonPressed OnMoveButtonPressed;
    public void PressMoveButton()
    {
        OnMoveButtonPressed();
    }
}
