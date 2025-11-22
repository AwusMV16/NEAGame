using UnityEngine;

public class Billboard2D : MonoBehaviour
{
    void LateUpdate()
    {
        transform.rotation = Quaternion.identity; // lock rotation
    }
}
