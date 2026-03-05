using System;
using UnityEngine;
using static CellType;

public class TilemapGameManager : MonoBehaviour
{
    [SerializeField] private TilemapBoardManager _board;

    private RuleSet _rules;

    public bool HasWon { get; private set; }

    // 클리어 알림 이벤트
    public event Action OnWon;

    private bool _isApplyingTransforms;

    public bool IsYou(ObjectType t) => HasRule(t, TextType.You);
    public bool IsStop(ObjectType t) => HasRule(t, TextType.Stop);
    public bool IsPush(ObjectType t) => HasRule(t, TextType.Push);
    public bool IsWin(ObjectType t) => HasRule(t, TextType.Win);
    public bool IsHot(ObjectType t) => HasRule(t, TextType.Hot);
    public bool IsMelt(ObjectType t) => HasRule(t, TextType.Melt);
    public bool IsDefeat(ObjectType t) => HasRule(t, TextType.Defeat);

    [Header("Stage Progress")]
    [SerializeField] private int _stageIndex0Based = 0; // Stage1=0, Stage2=1
    [SerializeField] private int _totalStages = 4;
    private bool _savedClearOnce = false;

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
        _savedClearOnce = false;
        RebuildRules();
    }

    private void RebuildRules()
    {
        if (_board == null) return;

        _rules = TilemapRuleScanner.Build(_board);

        ApplyObjectTransforms();

        // 룰이 바뀌면 승리상태는 다시 false (기존 로직 유지)
        HasWon = false;
    }

    private bool HasRule(ObjectType subject, TextType property)
        => _rules != null && _rules.Has(subject, property);

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
        if (HasWon) return; //이미 클리어면 중복 호출 방지

        var objs = _board.GetObjects(pos.x, pos.y);
        for (int i = 0; i < objs.Count; i++)
        {
            if (IsWin(objs[i]))
            {
                HasWon = true;
                if(!_savedClearOnce)
                { 
                    StageProgressIO.MarkCleared(_stageIndex0Based, _totalStages);
                    _savedClearOnce = true;
                }
                OnWon?.Invoke(); // UI가 뜨게 된다
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

        if (hasDefeat)
        {
            _board.RemoveObjectOnce(pos.x, pos.y, mover);
            return false;
        }

        if (hasHot && IsMelt(mover))
        {
            _board.RemoveObjectOnce(pos.x, pos.y, mover);
            return false;
        }

        return true;
    }
}