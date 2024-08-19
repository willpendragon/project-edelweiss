using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class CameraDistanceController : MonoBehaviour
{
    public List<GameObject> unitsOnBattlefield;
    // Call this method whenever a unit moves.
    public void Start()
    {
        SortUnits();
    }

    public void SortUnits()
    {
        Debug.Log("Sorting Units Z-Order");

        // Update the list of units on the battlefield.
        unitsOnBattlefield.Clear();
        unitsOnBattlefield.AddRange(FindGameObjectsInLayer(LayerMask.NameToLayer("Unit")));
        unitsOnBattlefield.AddRange(FindGameObjectsInLayer(LayerMask.NameToLayer("UnitMapIcon")));


        // Sort the list of units by their distance from the camera. The unit farthest from the camera gets the highest sorting order.
        unitsOnBattlefield.Sort((unit1, unit2) =>
            (Camera.main.transform.position - unit2.transform.position).sqrMagnitude
            .CompareTo((Camera.main.transform.position - unit1.transform.position).sqrMagnitude));

        // Now that the list is sorted, assign sorting orders where the first unit in the list gets the smallest sorting order
        // so it's rendered on top of others, and so on.
        for (int i = 0; i < unitsOnBattlefield.Count; i++)
        {
            if (unitsOnBattlefield[i].GetComponentInChildren<SpriteRenderer>() != null)
                unitsOnBattlefield[i].GetComponentInChildren<SpriteRenderer>().sortingOrder = i;
        }
    }

    GameObject[] FindGameObjectsInLayer(int layer)
    {
        GameObject[] foundObjects = FindObjectsOfType<GameObject>();
        List<GameObject> objectsInLayer = new List<GameObject>();

        foreach (GameObject obj in foundObjects)
        {
            if (obj.layer == layer)
            {
                objectsInLayer.Add(obj);
            }
        }

        return objectsInLayer.ToArray();
    }
}
