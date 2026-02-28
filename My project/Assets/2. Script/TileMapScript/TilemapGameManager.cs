using UnityEngine;
using static CellType;

public class TilemapGameManager : MonoBehaviour
{
    [SerializeField] private TilemapBoardManager _board;

    private RuleSet _rules;
    public bool HasWon { get; private set; }

    public bool IsYou(ObjectType t) => HasRule(t, TextType.You);
    public bool IsStop(ObjectType t) => HasRule(t, TextType.Stop);
    public bool IsPush(ObjectType t) => HasRule(t, TextType.Push);
    public bool IsWin(ObjectType t) => HasRule(t, TextType.Win);

    private void Awake()
    {
        if (_board == null) _board = FindFirstObjectByType<TilemapBoardManager>();
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
        _rules = TilemapRuleScanner.Build(_board);

        Debug.Log($"[Tilemap] Rules={_rules.RuleCount}");
        foreach (var kv in _rules.Pairs)
            foreach (var prop in kv.Value)
                Debug.Log($"{kv.Key} IS {prop}");

        HasWon = false;
    }

    private bool HasRule(ObjectType subject, TextType property)
        => _rules != null && _rules.Has(subject, property);

    public void CheckWinAt(Vector2Int pos)
    {
        // YOU가 이동한 칸에 WIN 오브젝트가 있으면 승리
        var obj = _board.GetObject(pos.x, pos.y);
        if (obj != ObjectType.None && IsWin(obj))
        {
            HasWon = true;
            Debug.Log("[Tilemap] YOU touched WIN!");
        }
    }
}