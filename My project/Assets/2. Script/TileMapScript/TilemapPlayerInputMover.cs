using System;
using System.Collections.Generic;
using UnityEngine;
using static CellType;

public class TilemapPlayerInputMover : MonoBehaviour
{
    [SerializeField] private TilemapBoardManager _board;
    [SerializeField] private TilemapGameManager _gm;

    private void Awake()
    {
        if (_board == null) _board = FindFirstObjectByType<TilemapBoardManager>();
        if (_gm == null) _gm = FindFirstObjectByType<TilemapGameManager>();
    }

    private void Update()
    {
        if (_gm != null && _gm.HasWon) return;

        Vector2Int dir = GetDirDown();
        if (dir == Vector2Int.zero) return;

        // 모든 YOU 오브젝트 수집
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

        // 이동 방향 기준으로 앞쪽부터 처리 (겹침 방지)
        movers.Sort((a, b) =>
        {
            if (dir == Vector2Int.right) return b.pos.x.CompareTo(a.pos.x);
            if (dir == Vector2Int.left) return a.pos.x.CompareTo(b.pos.x);
            if (dir == Vector2Int.up) return b.pos.y.CompareTo(a.pos.y);
            return a.pos.y.CompareTo(b.pos.y); // down
        });

        foreach (var m in movers)
        {
            TryMoveYou(m.type, m.pos, dir);
        }
    }

    // =========================
    // YOU 이동 (텍스트는 항상 PUSH)
    // =========================
    private bool TryMoveYou(ObjectType mover, Vector2Int from, Vector2Int dir)
    {
        Vector2Int to = from + dir;
        if (!_board.InRange(to.x, to.y)) return false;

        // 1) 목적지에 오브젝트가 있으면 처리
        var targetObj = _board.GetObject(to.x, to.y);
        if (targetObj != ObjectType.None)
        {
            if (_gm != null && _gm.IsStop(targetObj)) return false;

            if (_gm != null && _gm.IsPush(targetObj))
            {
                if (!TryShiftObjectAt(to, dir)) return false;
            }
            else
            {
                // STOP도 아니고 PUSH도 아닌 오브젝트는 "겹치지 못함" 처리로 막고 싶다면 false.
                // 원작 느낌(대부분은 겹치지 않음)을 원하면 아래 줄 유지.
                return false;
            }
        }

        // 2) 목적지에 텍스트가 있으면 무조건 밀기 (항상 PUSH)
        var targetText = _board.GetText(to.x, to.y);
        if (targetText != TextType.None)
        {
            if (!TryShiftTextAt(to, dir)) return false;
        }

        // 3) 이동
        _board.SetObject(to.x, to.y, mover);
        _board.SetObject(from.x, from.y, ObjectType.None);

        // 4) 승리 체크
        _gm?.CheckWinAt(to);

        return true;
    }

    // =========================
    // 텍스트 밀기 (항상 PUSH)
    // =========================
    private bool TryShiftTextAt(Vector2Int from, Vector2Int dir)
    {
        var movingText = _board.GetText(from.x, from.y);
        if (movingText == TextType.None) return true;

        Vector2Int to = from + dir;
        if (!_board.InRange(to.x, to.y)) return false;

        // 목적지 오브젝트 처리
        var targetObj = _board.GetObject(to.x, to.y);
        if (targetObj != ObjectType.None)
        {
            if (_gm != null && _gm.IsStop(targetObj)) return false;

            if (_gm != null && _gm.IsPush(targetObj))
            {
                if (!TryShiftObjectAt(to, dir)) return false;
            }
            else
            {
                // PUSH가 아닌 오브젝트는 텍스트도 겹치지 못하게 막음(원작 감성)
                return false;
            }
        }

        // 목적지 텍스트 처리(연쇄 밀기)
        var targetText = _board.GetText(to.x, to.y);
        if (targetText != TextType.None)
        {
            if (!TryShiftTextAt(to, dir)) return false;
        }

        // 이동
        _board.SetText(to.x, to.y, movingText);
        _board.SetText(from.x, from.y, TextType.None);

        return true;
    }

    // =========================
    // 오브젝트 밀기 (PUSH 대상만)
    // =========================
    private bool TryShiftObjectAt(Vector2Int from, Vector2Int dir)
    {
        var movingObj = _board.GetObject(from.x, from.y);
        if (movingObj == ObjectType.None) return true;

        Vector2Int to = from + dir;
        if (!_board.InRange(to.x, to.y)) return false;

        // 목적지 오브젝트 처리
        var targetObj = _board.GetObject(to.x, to.y);
        if (targetObj != ObjectType.None)
        {
            if (_gm != null && _gm.IsStop(targetObj)) return false;

            if (_gm != null && _gm.IsPush(targetObj))
            {
                if (!TryShiftObjectAt(to, dir)) return false;
            }
            else
            {
                return false;
            }
        }

        // 목적지 텍스트 처리(텍스트는 항상 PUSH)
        var targetText = _board.GetText(to.x, to.y);
        if (targetText != TextType.None)
        {
            if (!TryShiftTextAt(to, dir)) return false;
        }

        // 이동
        _board.SetObject(to.x, to.y, movingObj);
        _board.SetObject(from.x, from.y, ObjectType.None);

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