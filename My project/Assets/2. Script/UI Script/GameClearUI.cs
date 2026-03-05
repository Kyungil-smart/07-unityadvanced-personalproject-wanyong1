using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameClearUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private TilemapGameManager _gm;

    [Header("UI")]
    [SerializeField] private GameObject _clearPanel;
    [SerializeField] private Button _nextStageButton;

    [Header("Scenes")]
    [SerializeField] private string _stageSelectSceneName = "LevelSelect";

    private bool _shown;

    private void Awake()
    {
        if (_gm == null) _gm = FindFirstObjectByType<TilemapGameManager>();

        if (_clearPanel != null) _clearPanel.SetActive(false);

        if (_nextStageButton != null) _nextStageButton.onClick.AddListener(OnClickNextStage);
        
    }

    private void OnEnable()
    {
        if (_gm != null) _gm.OnWon += ShowClear;
    }

    private void OnDisable()
    {
        if (_gm != null) _gm.OnWon -= ShowClear;
    }

    private void ShowClear()
    {
        if (_shown) return;
        _shown = true;

        if (_clearPanel != null) _clearPanel.SetActive(true);

        // 원하면 정지(퍼즐게임 느낌)
        Time.timeScale = 0f;
    }

    private void OnClickNextStage()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(_stageSelectSceneName);
    }

    private void OnClickRetry()
    {
        Time.timeScale = 1f;
        var active = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(active);
    }
}