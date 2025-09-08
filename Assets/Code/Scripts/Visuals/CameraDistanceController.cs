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
        SortUnitsWrapper();
    }

    IEnumerator SortUnitsWrapper()
    {
        // Using a coroutine to delay the sorting of units after units spawned.
        yield return new WaitForSeconds(0.1f);
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

        // Flatten all SpriteRenderers from all units into a single list
        var allRenderers = unitsOnBattlefield
            .SelectMany(unit => unit.GetComponentsInChildren<SpriteRenderer>())
            .ToList();

        // Assign sorting order to SpriteRenderers
        for (int i = 0; i < allRenderers.Count; i++)
        {
            allRenderers[i].sortingOrder = i;
        }

        // Flatten all Canvas components from all units into a single list
        var allCanvases = unitsOnBattlefield
            .SelectMany(unit => unit.GetComponentsInChildren<Canvas>())
            .ToList();

        // Assign sorting order to Canvases (optionally, you can use a different offset or order)
        for (int i = 0; i < allCanvases.Count; i++)
        {
            allCanvases[i].sortingOrder = i;
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
