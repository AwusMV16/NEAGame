using UnityEngine;

public class Rotate : MonoBehaviour
{
    [SerializeField] private float speed;
    Quaternion worldRotation;

    void Start()
    {
        worldRotation = transform.rotation;
    }

    void LateUpdate()
    {
        transform.rotation = worldRotation;
        transform.Rotate(Vector3.forward * speed * Time.deltaTime, Space.World);
        worldRotation = transform.rotation;
    }
}
