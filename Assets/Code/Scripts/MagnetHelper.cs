using UnityEngine;

public class MagnetHelper : MonoBehaviour
{
    public GameObject miniMagnetPrefab;
    public GameObject tileConveyorOverlayPrefab;
    private GameObject activeMagnet;

    public void OrientMagnet(Unit attacker, Unit defender)
    {
        // Instantiate the magnet prefab
        if (activeMagnet == null)
        {
            activeMagnet = Instantiate(miniMagnetPrefab);
        }
        // Position it above the attacker
        Vector3 attackerPosition = attacker.transform.position;
        Vector3 offset = new Vector3(0, 2f, 0);  // Raise the magnet above the attacker's head
        activeMagnet.transform.position = attackerPosition + offset;

        // Get the defender's position
        Vector3 defenderPosition = defender.transform.position;

        // Determine the direction the magnet should face
        Vector3 direction = (defenderPosition - attackerPosition).normalized;

        // Apply the LookRotation only on the X-Z plane to avoid flipping around Y-axis
        Vector3 lookDirection = new Vector3(direction.x, 0, direction.z);
        if (lookDirection != Vector3.zero)
        {
            activeMagnet.transform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
        }
    }
    public void DestroyMagnet()
    {
        //float timeToDestroyMagnet = 3f;
        Destroy(activeMagnet);
        activeMagnet = null;
    }
}
