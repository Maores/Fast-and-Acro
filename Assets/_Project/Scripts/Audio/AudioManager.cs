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
    [SerializeField] private AudioClip _coinPickupClip;
    [SerializeField] private AudioClip _powerUpClip;

    [Header("Music")]
    [SerializeField] private AudioClip _menuMusic;
    [SerializeField] private AudioClip _gameplayMusic;
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] [Range(0f, 1f)] private float _musicVolume = 0.3f;

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

    public void PlayCoinPickup()
    {
        PlayClip(_coinPickupClip);
    }

    public void PlayPowerUp()
    {
        PlayClip(_powerUpClip);
    }

    public void PlayMenuMusic()
    {
        PlayMusic(_menuMusic);
    }

    public void PlayGameplayMusic()
    {
        PlayMusic(_gameplayMusic);
    }

    public void StopMusic()
    {
        if (_musicSource != null)
            _musicSource.Stop();
    }

    private void PlayMusic(AudioClip clip)
    {
        if (_musicSource == null || clip == null) return;
        if (_musicSource.clip == clip && _musicSource.isPlaying) return;
        _musicSource.clip = clip;
        _musicSource.loop = true;
        _musicSource.volume = _musicVolume;
        _musicSource.Play();
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
        if (clip != null && _sfxSource != null)
        {
            _sfxSource.PlayOneShot(clip);
        }
    }
}
