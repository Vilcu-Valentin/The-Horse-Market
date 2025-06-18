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
            DontDestroyOnLoad(gameObject);
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
        src.volume = volume;
        src.pitch = 1f + Random.Range(-pitchVariance, pitchVariance);
        src.Play();

        // destroy when done (accounting for pitch)
        Destroy(src.gameObject, clip.length / src.pitch);
    }
}
