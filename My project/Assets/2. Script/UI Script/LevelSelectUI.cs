using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelectUI : MonoBehaviour
{
    [Header("Stage Buttons (Stage1~)")]
    [SerializeField] private Button[] stageButtons;

    [Header("Scene Names (same order as buttons)")]
    [SerializeField] private string[] stageSceneNames;

    [Header("Back Button")]
    [SerializeField] private Button backButton;
    [SerializeField] private string mainSceneName = "";

    [Header("Alpha")]
    [SerializeField, Range(0f, 1f)] private float clearedAlpha = 1f;      // 클리어: 불투명
    [SerializeField, Range(0f, 1f)] private float unclearedAlpha = 0.35f; // 미클리어: 반투명

    private StageProgress _progress;

    private void Awake()
    {
        // 스테이지 버튼 클릭 이벤트 자동 연결
        for (int i = 0; i < stageButtons.Length; i++)
        {
            int idx = i; // 캡처 주의
            stageButtons[i].onClick.RemoveAllListeners();
            stageButtons[i].onClick.AddListener(() => OnClickStage(idx));
        }

        // Back 버튼
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(OnClickBack);
        }
    }

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        int stageCount = stageButtons.Length;
        _progress = StageProgressIO.LoadOrCreate(stageCount);

        for (int i = 0; i < stageCount; i++)
        {
            bool isCleared = _progress.cleared[i];
            bool isUnlocked = (i == 0) || _progress.cleared[i - 1];

            ApplyAlphaAll(stageButtons[i], isCleared ? clearedAlpha : unclearedAlpha);
            stageButtons[i].interactable = isUnlocked;
        }
    }

    private void OnClickStage(int index)
    {
        if (stageSceneNames == null || index < 0 || index >= stageSceneNames.Length)
        {
            Debug.LogError($"[LevelSelectUI] stageSceneNames 매핑이 부족해. index={index}");
            return;
        }

        string sceneName = stageSceneNames[index];
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError($"[LevelSelectUI] sceneName 비어있음. index={index}");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    private void OnClickBack()
    {
        if (string.IsNullOrWhiteSpace(mainSceneName))
        {
            Debug.LogError("[LevelSelectUI] mainSceneName 비어있음.");
            return;
        }
        SceneManager.LoadScene(mainSceneName);
    }

    private void ApplyAlphaAll(Button btn, float alpha)
    {
        if (btn == null) return;

        // 1) CanvasGroup으로 전체 알파 (텍스트 포함)
        var cg = btn.GetComponent<CanvasGroup>();
        if (cg == null) cg = btn.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = alpha;

    }

}