using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Clips")]
    public AudioClip wing;
    public AudioClip point;
    public AudioClip hit;
    public AudioClip die;
    public AudioClip swoosh;

    AudioSource source;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        source = GetComponent<AudioSource>();
    }

    public void PlayWing()   => Play(wing);
    public void PlayPoint()  => Play(point);
    public void PlaySwoosh() => Play(swoosh);

    // Hit plays immediately; die plays 0.3 s later.
    public void PlayHit()
    {
        Play(hit);
        StartCoroutine(PlayAfterDelay(die, 0.3f));
    }

    void Play(AudioClip clip)
    {
        if (clip == null || source == null) return;
        source.PlayOneShot(clip);
    }

    IEnumerator PlayAfterDelay(AudioClip clip, float delay)
    {
        yield return new WaitForSeconds(delay);
        Play(clip);
    }
}
