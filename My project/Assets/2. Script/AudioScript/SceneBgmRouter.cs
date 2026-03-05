using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class SceneBgmRouter : MonoBehaviour
{
    [Serializable]
    public struct SceneBgm
    {
        public string sceneName;     // 반드시 Build Settings의 Scene 이름과 동일
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
                // 아무것도 안 함: 기존 BGM 유지 (가장 안전)
                return;
            }
            // 원하면 여기서 StopBgm 같은 정책을 넣을 수도 있는데,
            // 지금 요청은 Fallback 없애는 거라 기본은 "유지"가 안전.
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