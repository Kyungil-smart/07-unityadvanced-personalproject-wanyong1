using UnityEngine;
using static CellType;

public class GameManager : MonoBehaviour
{
    [SerializeField] private BoardManager _board;

    private RuleSet _rules;

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

    // 다른 스크립트에서 규칙 조회할 수 있게 (이동/충돌 때 씀)
    public bool HasRule(ObjectType subject, TextType prop)
        => _rules != null && _rules.Has(subject, prop);
}