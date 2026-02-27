using UnityEngine;
using static CellType;

public class GameManager : MonoBehaviour
{
    [SerializeField] private BoardManager _board;

    private RuleSet _rules;
    public bool HasWon { get; private set; }
    public bool IsYou(ObjectType t) => HasRule(t, TextType.You);
    public bool IsStop(ObjectType t) => HasRule(t, TextType.Stop);
    public bool IsPush(ObjectType t) => HasRule(t, TextType.Push);
    public bool IsWin(ObjectType t) => HasRule(t, TextType.Win);

    private void Awake()
    {
        if (_board == null) _board = FindFirstObjectByType<BoardManager>();
    }

    private void OnEnable()
    {
        if (_board != null)
            _board.OnBoardChanged += RebuildRules;
    }

    private void OnDisable()
    {
        if (_board != null)
            _board.OnBoardChanged -= RebuildRules;
    }

    private void Start()
    {
        RebuildRules();
    }

    private void RebuildRules()
    {
        _rules = RuleScanner.Build(_board);

        Debug.Log($"Rules={_rules.RuleCount}");

        // 보기 좋게 출력
        foreach (var kv in _rules.Pairs)
        {
            foreach (var prop in kv.Value)
                Debug.Log($"{kv.Key} IS {prop}");
        }
    }
    public void TriggerWin(ObjectType youType, Vector2Int cell)
    {
        if (HasWon) return;
        HasWon = true;

        Debug.Log($"WIN! YOU={youType}, Cell={cell}");

        // TODO: 여기서 나중에
        // - 입력 잠금
        // - 클리어 UI
        // - 다음 레벨 로드
    }
    // 다른 스크립트에서 규칙 조회
    public bool HasRule(ObjectType subject, TextType prop)
        => _rules != null && _rules.Has(subject, prop);
}