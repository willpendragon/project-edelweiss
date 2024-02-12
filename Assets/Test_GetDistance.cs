using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_GetDistance : MonoBehaviour
{
    public GridMovementController gridMovementController;
    public TileController tileA;
    public TileController tileB;
    public void OnButtonClick()
    {
        int distance = gridMovementController.GetDistance(tileA, tileB);
        Debug.Log(distance);
        // Do something with the distance
    }
}
