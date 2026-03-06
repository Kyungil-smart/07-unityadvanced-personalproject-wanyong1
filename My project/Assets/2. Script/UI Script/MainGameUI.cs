using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainGameUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject _mainPanel;
    [SerializeField] private GameObject _helpPanel;

    [Header("Buttons")]
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _helpButton;
    [SerializeField] private Button _quitButton;
    [SerializeField] private Button _backButton;

    [Header("Scene (Optional)")]
    [Tooltip("Start 버튼을 눌렀을 때 이동할 씬 이름. 비워두면 이동 안 함.")]
    [SerializeField] private string _levelSelectSceneName = "LevelSelect";

    private void Awake()
    {
      
        ShowMain();

        SafeRemoveAll(_startButton);
        SafeRemoveAll(_helpButton);
        SafeRemoveAll(_quitButton);
        SafeRemoveAll(_backButton);

        // 버튼 이벤트 연결
        if (_startButton != null) _startButton.onClick.AddListener(OnClickStart);
        if (_helpButton != null) _helpButton.onClick.AddListener(OnClickHelp);
        if (_quitButton != null) _quitButton.onClick.AddListener(OnClickQuit);
        if (_backButton != null) _backButton.onClick.AddListener(OnClickBack);
    }

    private void Update()
    {
        // HelpPanel 켜져 있을 때 ESC 누르면 뒤로
        if (_helpPanel != null && _helpPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            ShowMain();
        }
    }

    private void ShowMain()
    {
        if (_mainPanel != null) _mainPanel.SetActive(true);
        if (_helpPanel != null) _helpPanel.SetActive(false);
    }

    private void ShowHelp()
    {
        if (_mainPanel != null) _mainPanel.SetActive(false);
        if (_helpPanel != null) _helpPanel.SetActive(true);
    }

    private void OnClickStart()
    {
        // 레벨 선택 씬으로 이동
        if (!string.IsNullOrEmpty(_levelSelectSceneName))
        {
            SceneManager.LoadScene(_levelSelectSceneName);
        }
        else
        {
            Debug.Log("[MainGameUI] levelSelectSceneName is empty. No scene loaded.");
        }
    }

    private void OnClickHelp()
    {
        ShowHelp();
    }

    private void OnClickBack()
    {
        ShowMain();
    }

    private void OnClickQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void SafeRemoveAll(Button btn)
    {
        if (btn != null) btn.onClick.RemoveAllListeners();
    }
}