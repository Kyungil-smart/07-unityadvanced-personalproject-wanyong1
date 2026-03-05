using System;
using System.Collections.Generic;
using UnityEngine;
using static CellType;

public class TilemapPlayerInputMover : MonoBehaviour
{
    [SerializeField] private TilemapBoardManager _board;
    [SerializeField] private TilemapGameManager _gm;
    [SerializeField] private TilemapUndoManager _undo;

    private void Awake()
    {
        if (_board == null) _board = FindFirstObjectByType<TilemapBoardManager>();
        if (_gm == null) _gm = FindFirstObjectByType<TilemapGameManager>();
        if (_undo == null) _undo = FindFirstObjectByType<TilemapUndoManager>();
    }

    private void Update()
    {
        if (_gm != null && _gm.HasWon) return;

        Vector2Int dir = GetDirDown();
        if (dir == Vector2Int.zero) return;

        var movers = new List<(ObjectType type, Vector2Int pos)>();

        foreach (ObjectType t in Enum.GetValues(typeof(ObjectType)))
        {
            if (t == ObjectType.None) continue;
            if (_gm == null || !_gm.IsYou(t)) continue;

            var list = _board.FindAll(t);
            foreach (var p in list)
                movers.Add((t, p));
        }

        if (movers.Count == 0) return;

        _undo?.BeginMove();   //이동 처리 전 스냅샷

        // 방향 기준 정렬(기존 유지)
        movers.Sort((a, b) =>
        {
            if (dir == Vector2Int.right) return b.pos.x.CompareTo(a.pos.x);
            if (dir == Vector2Int.left) return a.pos.x.CompareTo(b.pos.x);
            if (dir == Vector2Int.up) return b.pos.y.CompareTo(a.pos.y);
            return a.pos.y.CompareTo(b.pos.y);
        });

        // 그 턴에 실제로 이동이 1번이라도 성공하면 한 번만 소리
        bool anyMoved = false;

        foreach (var m in movers)
            anyMoved |= TryMoveYou(m.type, m.pos, dir);

        _undo?.EndMove();

        if (anyMoved)
            AudioManager.Instance?.PlayMove();
    }

    private bool TryMoveYou(ObjectType mover, Vector2Int from, Vector2Int dir)
    {
        Vector2Int to = from + dir;
        if (!_board.InRange(to.x, to.y)) return false;

        // 1) 텍스트는 항상 밀기(칸당 1개 유지)
        var targetText = _board.GetText(to.x, to.y);
        if (targetText != TextType.None)
        {
            if (!TryShiftTextAt(to, dir)) return false;
        }

        // 2) 목적지 오브젝트 처리
        var targets = _board.GetObjects(to.x, to.y);

        // STOP 하나라도 있으면 못감
        for (int i = 0; i < targets.Count; i++)
        {
            if (_gm != null && _gm.IsStop(targets[i]))
                return false;
        }

        // PUSH 전부 밀기
        if (!TryShiftAllPushObjectsAt(to, dir))
            return false;

        // 3) 이동(겹침): 목적지는 유지, mover만 옮김
        _board.MoveObjectOnce(from.x, from.y, to.x, to.y, mover);
        if (mover == ObjectType.Baba)
            _board.SetBabaFacing(to.x, to.y, dir);

        // 3.5) 이동 후 상호작용 처리 (DEFEAT, HOT+MELT 등)
        bool alive = _gm == null || _gm.ResolveAfterMove(to, mover);
        if (!alive) return true;

        // 4) 승리 체크
        _gm?.CheckWinAt(to);

        return true;
    }

    // 텍스트 밀기(항상 PUSH) - 칸당 1개 유지
    private bool TryShiftTextAt(Vector2Int from, Vector2Int dir)
    {
        var movingText = _board.GetText(from.x, from.y);
        if (movingText == TextType.None) return true;

        Vector2Int to = from + dir;
        if (!_board.InRange(to.x, to.y)) return false;

        // 목적지 STOP 있으면 막힘
        var objs = _board.GetObjects(to.x, to.y);
        for (int i = 0; i < objs.Count; i++)
            if (_gm != null && _gm.IsStop(objs[i])) return false;

        // 목적지 PUSH는 먼저 밀기
        if (!TryShiftAllPushObjectsAt(to, dir)) return false;

        // 목적지 텍스트 연쇄
        var t = _board.GetText(to.x, to.y);
        if (t != TextType.None)
            if (!TryShiftTextAt(to, dir)) return false;

        _board.SetText(to.x, to.y, movingText);
        _board.SetText(from.x, from.y, TextType.None);
        return true;
    }

    // 오브젝트 밀기: from 칸에 있는 PUSH 전부를 1칸 밀기(연쇄)
    private bool TryShiftAllPushObjectsAt(Vector2Int from, Vector2Int dir)
    {
        var here = _board.GetObjects(from.x, from.y);

        // from 칸의 PUSH들 스냅샷
        var pushList = new List<ObjectType>();
        for (int i = 0; i < here.Count; i++)
            if (_gm != null && _gm.IsPush(here[i]))
                pushList.Add(here[i]);

        if (pushList.Count == 0) return true;

        Vector2Int to = from + dir;
        if (!_board.InRange(to.x, to.y)) return false;

        // 목적지 STOP이면 불가
        var targets = _board.GetObjects(to.x, to.y);
        for (int i = 0; i < targets.Count; i++)
            if (_gm != null && _gm.IsStop(targets[i])) return false;

        // 목적지에도 PUSH가 있으면 먼저 연쇄 밀기
        if (!TryShiftAllPushObjectsAt(to, dir)) return false;

        // from의 PUSH들을 전부 to로 이동
        for (int i = 0; i < pushList.Count; i++)
            _board.MoveObjectOnce(from.x, from.y, to.x, to.y, pushList[i]);

        return true;
    }

    private Vector2Int GetDirDown()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) return Vector2Int.right;
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) return Vector2Int.left;
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) return Vector2Int.up;
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) return Vector2Int.down;
        return Vector2Int.zero;
    }
}