using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioMixer masterMixer;

    // Call this to change master volume, 0f = full, -80f = mute
    public void SetMasterVolume(float volume)
    {
        // Convert linear 0-1 slider to decibels
        float dB = Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f;
        masterMixer.SetFloat("MasterVolume", dB);
    }
}