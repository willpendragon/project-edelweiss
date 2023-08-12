using UnityEngine;
public class OrbitCamera : MonoBehaviour
{
    public Transform target;

    [Range(1, 10)]
    public float speed = 5f;

    private float m_Distance;
    private Vector2 m_Rotation = Vector2.zero;

    private Vector3 m_DistanceVector;
    void Start()
    {
        m_Distance = -this.transform.position.z;
        m_DistanceVector = new Vector3(0.0f, 0.0f, -m_Distance);
        m_Rotation = this.transform.localEulerAngles;
        this.Rotate(m_Rotation);
    }
    void LateUpdate()
    {
        if (target)
        {
            if (Input.GetMouseButton(1))
            {
                m_Rotation.x += Input.GetAxis("Mouse X") * speed;
                m_Rotation.y += -Input.GetAxis("Mouse Y") * speed;
                this.Rotate(m_Rotation);
            }
            m_Distance = m_Distance - Input.GetAxis("Mouse ScrollWheel") * speed;
            Zoom();
        }
    }
    void Rotate(Vector2 angle)
    {
        // Transform angle in degree in quaternion form used by Unity for rotation.
        Quaternion rotation = Quaternion.Euler(angle.y, angle.x, 0.0f);
        // The new position is the target position + the distance vector of the camera
        // rotated at the specified angle.
        Vector3 position = rotation * m_DistanceVector + target.position;
        // Update the rotation and position of the camera.
        transform.rotation = rotation;
        transform.position = position;
    }
    void Zoom()
    {
        m_DistanceVector = new Vector3(0.0f, 0.0f, -m_Distance);
        Rotate(m_Rotation);
    }
}