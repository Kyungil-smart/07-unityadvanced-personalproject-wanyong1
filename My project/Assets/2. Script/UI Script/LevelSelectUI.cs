using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelectUI : MonoBehaviour
{
    [Header("Stage Buttons (Stage1~)")]
    [SerializeField] private Button[] stageButtons;

    [Header("Scene Names (same order as buttons)")]
    [SerializeField] private string[] stageSceneNames;

    [Header("Alpha")]
    [SerializeField, Range(0f, 1f)] private float clearedAlpha = 1f;      // 클리어: 불투명
    [SerializeField, Range(0f, 1f)] private float unclearedAlpha = 0.35f; // 미클리어: 반투명

    private StageProgress _progress;

    private void Awake()
    {
        // 버튼 클릭 이벤트 자동 연결 (Inspector에서 OnClick 안 넣어도 됨)
        for (int i = 0; i < stageButtons.Length; i++)
        {
            int idx = i; // 캡처 주의
            stageButtons[i].onClick.RemoveAllListeners();
            stageButtons[i].onClick.AddListener(() => OnClickStage(idx));
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
            bool isUnlocked = (i == 0) || _progress.cleared[i - 1]; // 이전 스테이지 클리어해야 오픈

            ApplyAlphaByButtonColors(stageButtons[i], isCleared ? clearedAlpha : unclearedAlpha);
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

    private void ApplyAlphaByButtonColors(Button btn, float alpha)
    {
        var cb = btn.colors;

        cb.normalColor = WithAlpha(cb.normalColor, alpha);
        cb.highlightedColor = WithAlpha(cb.highlightedColor, alpha);
        cb.pressedColor = WithAlpha(cb.pressedColor, alpha);
        cb.selectedColor = WithAlpha(cb.selectedColor, alpha);
        cb.disabledColor = WithAlpha(cb.disabledColor, Mathf.Clamp01(alpha * 0.7f));

        btn.colors = cb;
    }

    private Color WithAlpha(Color c, float a)
    {
        c.a = a;
        return c;
    }
}