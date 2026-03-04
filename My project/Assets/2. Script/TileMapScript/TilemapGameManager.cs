using UnityEngine;
using static CellType;

public class TilemapGameManager : MonoBehaviour
{
    [SerializeField] private TilemapBoardManager _board;

    private RuleSet _rules;
    public bool HasWon { get; private set; }

    private bool _isApplyingTransforms;

    public bool IsYou(ObjectType t) => HasRule(t, TextType.You);
    public bool IsStop(ObjectType t) => HasRule(t, TextType.Stop);
    public bool IsPush(ObjectType t) => HasRule(t, TextType.Push);
    public bool IsWin(ObjectType t) => HasRule(t, TextType.Win);
    public bool IsHot(ObjectType t) => HasRule(t, TextType.Hot);
    public bool IsMelt(ObjectType t) => HasRule(t, TextType.Melt);
    public bool IsDefeat(ObjectType t) => HasRule(t, TextType.Defeat);

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
        if (_board == null) return;

        _rules = TilemapRuleScanner.Build(_board);

        Debug.Log($"[Tilemap] Rules={_rules.RuleCount}");
        foreach (var kv in _rules.Pairs)
            foreach (var prop in kv.Value)
                Debug.Log($"{kv.Key} IS {prop}");

        foreach (var tr in _rules.TransformPairs)
            Debug.Log($"{tr.Key} IS {tr.Value}");

        ApplyObjectTransforms();

        HasWon = false;
    }

    private bool HasRule(ObjectType subject, TextType property)
        => _rules != null && _rules.Has(subject, property);

    // Object IS Object 적용
    // 예) BABA IS WALL 이면 보드의 Baba를 Wall로 치환
    private void ApplyObjectTransforms()
    {
        if (_board == null || _rules == null) return;
        if (_isApplyingTransforms) return;

        _isApplyingTransforms = true;

        bool changed = false;

        _board.BeginBatch();
        for (int y = 0; y < _board.Height; y++)
            for (int x = 0; x < _board.Width; x++)
            {
                var list = _board.GetObjects(x, y);
                if (list.Count == 0) continue;

              
                for (int i = 0; i < list.Count; i++)
                {
                    var from = list[i];
                    if (_rules.TryGetTransform(from, out var to) && to != from)
                    {
                        if (_board.RemoveObjectOnce(x, y, from))
                        {
                            _board.AddObject(x, y, to);
                            changed = true;
                        }
                    }
                }
            }
        _board.EndBatch(notify: changed);

        _isApplyingTransforms = false;
    }

    public void CheckWinAt(Vector2Int pos)
    {
        var objs = _board.GetObjects(pos.x, pos.y);
        for (int i = 0; i < objs.Count; i++)
        {
            if (IsWin(objs[i]))
            {
                HasWon = true;
                Debug.Log("[Tilemap] YOU touched WIN!");
                return;
            }
        }
    }
    public bool ResolveAfterMove(Vector2Int pos, ObjectType mover)
    {
        if (_board == null) return true;

        var objs = _board.GetObjects(pos.x, pos.y);

        bool hasDefeat = false;
        bool hasHot = false;

        for (int i = 0; i < objs.Count; i++)
        {
            var o = objs[i];
            if (IsDefeat(o)) hasDefeat = true;
            if (IsHot(o)) hasHot = true;
        }

        // 1) DEFEAT: 밟으면 죽음
        if (hasDefeat)
        {
            _board.RemoveObjectOnce(pos.x, pos.y, mover);
            return false;
        }

        // 2) HOT + MELT: mover가 MELT면 HOT 있는 칸에서 죽음
        if (hasHot && IsMelt(mover))
        {
            _board.RemoveObjectOnce(pos.x, pos.y, mover);
            return false;
        }

        return true;
    }

}