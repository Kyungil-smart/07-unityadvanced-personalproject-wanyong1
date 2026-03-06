using UnityEngine;

public sealed class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sources")]
    [SerializeField] private AudioSource _bgmSource;
    [SerializeField] private AudioSource _sfxSource;

    [Header("Default Volume")]
    [SerializeField, Range(0f, 1f)] private float _bgmVolume = 0.7f;
    [SerializeField, Range(0f, 1f)] private float _sfxVolume = 0.8f;

    [Header("SFX Clips")]
    [SerializeField] private AudioClip _moveClip;
    [SerializeField] private AudioClip _deadClip;
    [SerializeField] private AudioClip _clearClip;

    public float BgmVolume
    {
        get => _bgmSource ? _bgmSource.volume : 0f;
        set { if (_bgmSource) _bgmSource.volume = Mathf.Clamp01(value); }
    }

    public float SfxVolume
    {
        get => _sfxSource ? _sfxSource.volume : 0f;
        set { if (_sfxSource) _sfxSource.volume = Mathf.Clamp01(value); }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureSources();
        BgmVolume = _bgmVolume;
        SfxVolume = _sfxVolume;
    }

    private void EnsureSources()
    {
        // BGM
        if (_bgmSource == null)
        {
            _bgmSource = gameObject.AddComponent<AudioSource>();
        }
        _bgmSource.playOnAwake = false;
        _bgmSource.loop = true;

        if (_sfxSource == null)
        {
            var child = new GameObject("SFX_Source");
            child.transform.SetParent(transform);
            _sfxSource = child.AddComponent<AudioSource>();
        }
        _sfxSource.playOnAwake = false;
        _sfxSource.loop = false;

        _sfxSource.spatialBlend = 0f;
        _bgmSource.spatialBlend = 0f;
    }

    public void PlayBgm(AudioClip clip, bool restart = false)
    {
        if (clip == null || _bgmSource == null) return;

        // 같은 클립이고 이미 재생 중이면 그대로 유지(기본)
        if (!restart && _bgmSource.isPlaying && _bgmSource.clip == clip)
            return;

        _bgmSource.Stop();
        _bgmSource.clip = clip;
        _bgmSource.Play();
    }

    public void StopBgm()
    {
        if (_bgmSource == null) return;
        _bgmSource.Stop();
    }

    public void PlaySfx(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null || _sfxSource == null) return;
        _sfxSource.PlayOneShot(clip, Mathf.Clamp01(volumeScale));
    }

    //SFX helpers
    public void PlayMove(float volumeScale = 1f) => PlaySfx(_moveClip, volumeScale);
    public void PlayDead(float volumeScale = 1f) => PlaySfx(_deadClip, volumeScale);
    public void PlayClear(float volumeScale = 1f) => PlaySfx(_clearClip, volumeScale);

    public void PauseBgm()
    {
        if (_bgmSource == null) return;
        if (_bgmSource.isPlaying) _bgmSource.Pause();
    }

    public void ResumeBgm()
    {
        if (_bgmSource == null) return;
        _bgmSource.UnPause();
    }
}