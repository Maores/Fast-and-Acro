using UnityEngine;

/// <summary>
/// Simple audio manager for one-shot sound effects.
/// Assign AudioClips in the Inspector. Uses PlayOneShot (no GC alloc).
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    [Header("Sound Effects")]
    [SerializeField] private AudioClip _collisionClip;
    [SerializeField] private AudioClip _levelCompleteClip;
    [SerializeField] private AudioClip _gameOverClip;
    [SerializeField] private AudioClip _buttonClickClip;
    [SerializeField] private AudioClip _laneSwitchClip;

    [Header("Engine")]
    [SerializeField] private AudioSource _engineSource;

    private AudioSource _sfxSource;

    private void Awake()
    {
        _sfxSource = GetComponent<AudioSource>();
    }

    public void PlayCollision()
    {
        PlayClip(_collisionClip);
    }

    public void PlayLevelComplete()
    {
        PlayClip(_levelCompleteClip);
        StopEngine();
    }

    public void PlayGameOver()
    {
        PlayClip(_gameOverClip);
        StopEngine();
    }

    public void PlayButtonClick()
    {
        PlayClip(_buttonClickClip);
    }

    public void PlayLaneSwitch()
    {
        PlayClip(_laneSwitchClip);
    }

    public void StartEngine()
    {
        if (_engineSource != null && !_engineSource.isPlaying)
        {
            _engineSource.loop = true;
            _engineSource.Play();
        }
    }

    public void StopEngine()
    {
        if (_engineSource != null && _engineSource.isPlaying)
        {
            _engineSource.Stop();
        }
    }

    private void PlayClip(AudioClip clip)
    {
        if (clip != null)
        {
            _sfxSource.PlayOneShot(clip);
        }
    }
}
