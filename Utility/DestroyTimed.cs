using UnityEngine;

public class DestroyTimed : MonoBehaviour
{
    [SerializeField] private float time;
    void Start()
    {
        Destroy(gameObject, time);
    }
}
