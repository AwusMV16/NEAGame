using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioMixer masterMixer;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource NormalImpactSource;
    [SerializeField] private AudioSource DamageImpactSource;
    
    public void PlayImpact(bool isDamage, Vector3 pos)
    {
        AudioSource src = isDamage ? DamageImpactSource : NormalImpactSource;

        src.transform.position = pos;
        src.pitch = Random.Range(0.95f, 1.05f);
        src.PlayOneShot(src.clip);
    }

    // Call this to change master volume, 0f = full, -80f = mute
    public void SetMasterVolume(float volume)
    {
        // Convert linear 0-1 slider to decibels
        float dB = Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f;
        masterMixer.SetFloat("MasterVolume", dB);
    }
}