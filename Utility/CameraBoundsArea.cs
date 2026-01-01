    using UnityEngine;
    using Unity.Cinemachine;

    public class CameraBoundsArea : MonoBehaviour
    {
        [SerializeField] private Collider2D boundsCollider;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            var confiner = FindFirstObjectByType<CinemachineConfiner2D>();
            // confiner.enabled = true;
            confiner.BoundingShape2D = boundsCollider;
            confiner.InvalidateBoundingShapeCache();

        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            var confiner = FindFirstObjectByType<CinemachineConfiner2D>();
            // confiner.enabled = false;
            confiner.BoundingShape2D = GameObject.FindWithTag("Camera Bounds").GetComponent<Collider2D>();
            confiner.InvalidateBoundingShapeCache();
        }
    }
