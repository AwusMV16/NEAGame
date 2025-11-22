using UnityEngine;
using UnityEngine.Rendering;

public class PostProcessFocusFix : MonoBehaviour
{
    private Volume volume;

    void Start()
    {
        volume = GetComponent<Volume>();
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && volume != null)
            StartCoroutine(RefreshVolume());
    }

    System.Collections.IEnumerator RefreshVolume()
    {
        yield return null; // wait one frame
        volume.enabled = false;
        yield return null;
        volume.enabled = true;
    }
}