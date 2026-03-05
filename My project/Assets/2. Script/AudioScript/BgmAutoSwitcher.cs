using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class BgmAutoSwitcher : MonoBehaviour
{
    [Serializable]
    public struct SceneBgm
    {
        public string sceneName;   // 예: "MainMenu", "Stage1", "Stage2"
        public AudioClip clip;
    }

    [Header("Exact Match Mapping (recommended)")]
    [SerializeField] private SceneBgm[] _sceneBgms;

    [Header("Fallback (optional)")]
    [Tooltip("씬 이름이 이 Prefix로 시작하면 Stage BGM을 사용")]
    [SerializeField] private string _stagePrefix = "Stage";
    [SerializeField] private AudioClip _stageFallbackClip;

    [Tooltip("씬 이름이 MainMenu면 이 BGM을 사용 (매핑에 없을 때)")]
    [SerializeField] private string _mainMenuSceneName = "MainMenu";
    [SerializeField] private AudioClip _mainMenuFallbackClip;

    [Header("Play Options")]
    [Tooltip("같은 클립이면 다시 시작할지")]
    [SerializeField] private bool _restartEvenIfSameClip = false;

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
        // 첫 진입 씬에도 적용
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (AudioManager.Instance == null) return;

        var clip = ResolveClip(scene.name);
        if (clip == null) return;

        AudioManager.Instance.PlayBgm(clip, restart: _restartEvenIfSameClip);
    }

    private AudioClip ResolveClip(string sceneName)
    {
        // 1) 정확히 이름 매칭 우선
        if (_sceneBgms != null)
        {
            for (int i = 0; i < _sceneBgms.Length; i++)
            {
                if (string.Equals(_sceneBgms[i].sceneName, sceneName, StringComparison.Ordinal))
                    return _sceneBgms[i].clip;
            }
        }

        // 2) MainMenu 폴백
        if (!string.IsNullOrEmpty(_mainMenuSceneName) &&
            string.Equals(sceneName, _mainMenuSceneName, StringComparison.Ordinal))
        {
            return _mainMenuFallbackClip;
        }

        // 3) Stage 폴백 (Stage1/Stage2/Stage_01 등)
        if (!string.IsNullOrEmpty(_stagePrefix) &&
            sceneName.StartsWith(_stagePrefix, StringComparison.Ordinal))
        {
            return _stageFallbackClip;
        }

        return null;
    }
}