using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Scene")]
    [SerializeField] private string mainMenuScene = "";

    private bool isPaused;

    private void Awake()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoMainMenu);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    private void PauseGame()
    {
        isPaused = true;

        if (pausePanel != null)
            pausePanel.SetActive(true);

        Time.timeScale = 0f;

        // BGM 일시정지
        AudioManager.Instance?.PauseBgm();
    }

    private void ResumeGame()
    {
        isPaused = false;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        Time.timeScale = 1f;

        // BGM 다시 재생
        AudioManager.Instance?.ResumeBgm();
    }

    private void GoMainMenu()
    {
        // 메뉴 나갈 때는 일시정지 풀고 씬 이동
        Time.timeScale = 1f;

        AudioManager.Instance?.StopBgm();

        SceneManager.LoadScene(mainMenuScene);
    }
}