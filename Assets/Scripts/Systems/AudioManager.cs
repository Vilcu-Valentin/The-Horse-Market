using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Music Settings")]
    [Tooltip("All your background music clips")]
    [SerializeField] private List<AudioClip> musicClips;
    [Tooltip("AudioSource used for BGM")]
    [SerializeField] private AudioSource musicSource;
    private float _musicVolume = 1f;
    private float _sfxVolume = 1f;

    [Header("SFX Settings")]
    [Tooltip("Prefab with an AudioSource (no clip assigned)")]
    [SerializeField] private AudioSource sfxSourcePrefab;
    [Tooltip("Default max pitch variance (± around 1.0)")]
    [SerializeField, Range(0f, 1f)] private float defaultPitchVariance = 0.1f;
    [Tooltip("Mixer group for SFX (optional)")]
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    private List<AudioClip> _shuffledPlaylist;
    private int _musicIndex = 0;


    void Awake()
    {
        // enforce singleton
        if (Instance == null)
        {
            Instance = this;
            InitPlaylist();
            StartCoroutine(MusicLoop());
        }
        else Destroy(gameObject);
}

    private void InitPlaylist()
    {
        // make a fresh shuffle
        _shuffledPlaylist = new List<AudioClip>(musicClips);
        for (int i = 0; i < _shuffledPlaylist.Count; i++)
        {
            int r = Random.Range(i, _shuffledPlaylist.Count);
            var tmp = _shuffledPlaylist[i];
            _shuffledPlaylist[i] = _shuffledPlaylist[r];
            _shuffledPlaylist[r] = tmp;
        }
        _musicIndex = 0;
    }

    // called by your slider
    public void SetMusicVolume(float volume)
    {
        _musicVolume = volume;
        musicSource.volume = _musicVolume;
    }

    // called by your slider
    public void SetSfxVolume(float volume)
    {
        _sfxVolume = volume;
        // you’ll apply this multiplier on every one-shot
    }

    private IEnumerator MusicLoop()
    {
        while (true)
        {
            if (_shuffledPlaylist.Count == 0) yield break;

            musicSource.clip = _shuffledPlaylist[_musicIndex];
            musicSource.Play();

            // wait for clip to finish
            yield return new WaitForSeconds(musicSource.clip.length);

            // advance, reshuffle if at end
            _musicIndex++;
            if (_musicIndex >= _shuffledPlaylist.Count)
            {
                InitPlaylist();
            }
        }
    }

    /// <summary>
    /// Play a one‐shot SFX with no pitch variance.
    /// </summary>
    public void PlaySound(AudioClip clip, float volume = 1f)
    {
        PlaySound(clip, volume, defaultPitchVariance);
    }

    /// <summary>
    /// Play a one‐shot SFX, randomly varying pitch ±variance.
    /// </summary>
    public void PlaySound(AudioClip clip, float volume, float pitchVariance)
    {
        if (clip == null) return;

        // spawn a temporary AudioSource
        var src = Instantiate(sfxSourcePrefab, transform);
        src.outputAudioMixerGroup = sfxMixerGroup;
        src.clip = clip;
        src.volume = volume * _sfxVolume;
        src.pitch = 1f + Random.Range(-pitchVariance, pitchVariance);
        src.Play();

        // destroy when done (accounting for pitch)
        Destroy(src.gameObject, clip.length / src.pitch);
    }
}
