using System;
using System.Collections.Generic;
using UnityEngine;
using static CellType;

public class PlayerInputMover : MonoBehaviour
{
    [SerializeField] private BoardManager _board;
    [SerializeField] private GameManager _gm;

    private void Awake()
    {
        if (_board == null) _board = FindFirstObjectByType<BoardManager>();
        if (_gm == null) _gm = FindFirstObjectByType<GameManager>();
    }

    private void Update()
    {
        if (_gm.HasWon) return;
        Vector2Int dir = GetDirDown();
        if (dir == Vector2Int.zero) return;

        // 1) 모든 오브젝트 수집
        var movers = new List<(ObjectType type, Vector2Int pos)>();

        foreach (ObjectType t in Enum.GetValues(typeof(ObjectType)))
        {
            if (t == ObjectType.None) continue;
            if (!_gm.IsYou(t)) continue; 

            var list = _board.FindAll(t); 
            foreach (var p in list)
                movers.Add((t, p));
        }

        if (movers.Count == 0) return;

        // 2) 겹침 방지: 진행 방향의 "앞쪽"부터 처리
        movers.Sort((a, b) =>
        {
            if (dir == Vector2Int.right) return b.pos.x.CompareTo(a.pos.x);
            if (dir == Vector2Int.left) return a.pos.x.CompareTo(b.pos.x);
            if (dir == Vector2Int.up) return b.pos.y.CompareTo(a.pos.y);
            return a.pos.y.CompareTo(b.pos.y); // down
        });

        // 3) 이동
        foreach (var m in movers)
        {
            // 이미 이동해서 그 칸의 타입이 바뀌었으면 스킵(중복 이동 방지)
            if (_board.GetObject(m.pos.x, m.pos.y) != m.type) continue;
            TryMoveObjectChain(m.type, m.pos, dir);
        }
    }

    Vector2Int GetDirDown()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow)) return Vector2Int.left;
        if (Input.GetKeyDown(KeyCode.RightArrow)) return Vector2Int.right;
        if (Input.GetKeyDown(KeyCode.UpArrow)) return Vector2Int.up;
        if (Input.GetKeyDown(KeyCode.DownArrow)) return Vector2Int.down;
        return Vector2Int.zero;
    }

    // =========================
    // TEXT PUSH (연쇄)
    // =========================
    bool HasText(Vector2Int p) => _board.GetText(p.x, p.y) != TextType.None;

    bool TryMoveTextChain(Vector2Int from, Vector2Int dir)
    {
        Vector2Int to = from + dir;
        if (!_board.InRange(to.x, to.y)) return false;

        // 다음 칸에 텍스트가 있으면 먼저 연쇄로 민다
        if (_board.GetText(to.x, to.y) != TextType.None)
        {
            if (!TryMoveTextChain(to, dir))
                return false;
        }

        // 오브젝트가 있어도 텍스트 이동 허용 (원작 감성)
        TextType t = _board.GetText(from.x, from.y);
        _board.SetText(from.x, from.y, TextType.None);
        _board.SetText(to.x, to.y, t);
        return true;
    }

    // =========================
    // OBJECT MOVE (연쇄 PUSH + WIN + STOP)
    // =========================
    bool TryMoveObjectChain(ObjectType moverType, Vector2Int from, Vector2Int dir)
    {
        Vector2Int to = from + dir;
        if (!_board.InRange(to.x, to.y)) return false;

        // 0) 이동하려는 칸에 텍스트가 있으면, 텍스트는 기본 PUSH 취급 → 먼저 민다(연쇄)
        if (HasText(to))
        {
            if (!TryMoveTextChain(to, dir))
                return false;
        }

        ObjectType target = _board.GetObject(to.x, to.y);

        // 1) 빈칸이면 이동
        if (target == ObjectType.None)
        {
            _board.SetObject(from.x, from.y, ObjectType.None);
            _board.SetObject(to.x, to.y, moverType);
            return true;
        }

        // 2) STOP이면 막힘
        if (_gm.IsStop(target)) return false;

        // 3) WIN이면 들어가면 승리
        if (_gm.IsWin(target) && _gm.IsYou(moverType))
        {
            _board.SetObject(from.x, from.y, ObjectType.None);
            _board.SetObject(to.x, to.y, ObjectType.Baba);

            _gm.TriggerWin(moverType, to); // 변경
            return true;
        }

        // 4) PUSH면 연쇄로 밀어본다
        if (_gm.IsPush(target))
        {
            if (!TryMoveObjectChain(target, to, dir))
                return false;

            _board.SetObject(from.x, from.y, ObjectType.None);
            _board.SetObject(to.x, to.y, moverType);
            return true;
        }

        return false;
    }
}