using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class SceneBgmRouter : MonoBehaviour
{
    [Serializable]
    public struct SceneBgm
    {
        public string sceneName; 
        public AudioClip bgmClip;
    }

    [Header("Scene -> BGM (Exact match only)")]
    [SerializeField] private SceneBgm[] _map;

    [Header("Options")]
    [SerializeField] private bool _restartEvenIfSameClip = false;
    [Tooltip("매핑에 없는 씬에서는 BGM을 건드리지 않음(그대로 유지).")]
    [SerializeField] private bool _doNothingIfNotMapped = true;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // 시작 씬에도 적용
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (AudioManager.Instance == null) return;

        if (TryGetClip(scene.name, out var clip))
        {
            AudioManager.Instance.PlayBgm(clip, restart: _restartEvenIfSameClip);
        }
        else
        {
            if (_doNothingIfNotMapped)
            {
                return;
            }

        }
    }

    private bool TryGetClip(string sceneName, out AudioClip clip)
    {
        if (_map != null)
        {
            for (int i = 0; i < _map.Length; i++)
            {
                if (string.Equals(_map[i].sceneName, sceneName, StringComparison.Ordinal))
                {
                    clip = _map[i].bgmClip;
                    return clip != null;
                }
            }
        }

        clip = null;
        return false;
    }
}