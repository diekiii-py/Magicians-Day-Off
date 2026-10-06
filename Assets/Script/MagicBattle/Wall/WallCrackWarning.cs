using System.Collections;
using UnityEngine;

/// <summary>
/// Plays the "something is cracking behind you" warning: a creaking/cracking sound that grows
/// louder, and an optional crack decal placed on the wall. Purely presentational - the actual
/// break is triggered by <see cref="WallBreakController"/> once the player punches the wall.
/// </summary>
public class WallCrackWarning : MonoBehaviour
{
    [Tooltip("Drag a looping creak/crack sound effect here. The AudioSource is created automatically.")]
    public AudioClip crackLoopClip;
    public float fadeInDuration = 3f;
    [Range(0f, 1f)] public float targetVolume = 0.8f;
    [Tooltip("Optional - a crack decal/visual GameObject to show on the wall. Leave empty to skip.")]
    public GameObject crackDecal;

    AudioSource _audioSource;
    Coroutine _routine;

    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null) _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;
        _audioSource.loop = true;
        _audioSource.spatialBlend = 1f;
    }

    public void BeginWarning(Vector3 worldPosition, Quaternion worldRotation)
    {
        if (crackDecal != null)
        {
            crackDecal.transform.SetPositionAndRotation(worldPosition, worldRotation);
            crackDecal.SetActive(true);
        }

        transform.position = worldPosition;

        if (crackLoopClip != null)
        {
            _audioSource.clip = crackLoopClip;
            _audioSource.volume = 0f;
            _audioSource.Play();
            if (_routine != null) StopCoroutine(_routine);
            _routine = StartCoroutine(FadeIn());
        }

        GameEvents.RaiseWallCrackStart();
    }

    public void StopWarning()
    {
        if (_routine != null) StopCoroutine(_routine);
        if (_audioSource != null) _audioSource.Stop();
        if (crackDecal != null) crackDecal.SetActive(false);
    }

    IEnumerator FadeIn()
    {
        float t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.deltaTime;
            _audioSource.volume = Mathf.Lerp(0f, targetVolume, t / fadeInDuration);
            yield return null;
        }
    }
}
