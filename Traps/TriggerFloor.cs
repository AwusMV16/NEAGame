using System.Collections;
using UnityEngine;

public class TriggerFloor : MonoBehaviour
{
    [SerializeField] private GameObject floor;
    private float AppearTime;
    private Coroutine dissipateRoutine;

    // Update is called once per frame
    void Update()
    {
        floor.SetActive(AppearTime > 0);
    }

    private IEnumerator Dissipate()
    {
        while (AppearTime > 0)
        {
            AppearTime = Mathf.Max(0, AppearTime - 0.1f);
            Debug.Log(AppearTime);
            yield return new WaitForSeconds(1f);
        }

        dissipateRoutine = null;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("PlayerBullet"))
        {
            AppearTime = Mathf.Min(0.2f, AppearTime + 0.05f);
            if (dissipateRoutine == null) dissipateRoutine = StartCoroutine(Dissipate());
        }
    }
}
