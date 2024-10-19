using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorBeltHelper : MonoBehaviour
{
    public GameObject conveyorBeltPrefab;
    public MeshRenderer conveyorBeltMesh;

    public void Start()
    {
        conveyorBeltMesh = conveyorBeltPrefab.GetComponent<MeshRenderer>();
        conveyorBeltMesh.enabled = false;
    }

    public void ManageConveyorBelt(float scrollSpeed)
    {
        conveyorBeltMesh.enabled = true;

        Material conveyorBeltMaterial = conveyorBeltMesh.material;

        if (conveyorBeltMaterial != null)
        {
            // Activates the shader on the Conveyor Belt with speed 1
            conveyorBeltMaterial.SetFloat("_ScrollSpeed", -1);
            StartCoroutine(DeactivateConveyorBelt());
        }
    }
    IEnumerator DeactivateConveyorBelt()
    {
        float deactivationTime = 0.5f;
        yield return new WaitForSeconds(deactivationTime);
        conveyorBeltMesh.enabled = false;
    }
}
