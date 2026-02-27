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
        Vector2Int dir = GetDirDown();
        if (dir == Vector2Int.zero) return;

        // 최소: Baba만 YOU인 케이스부터
        if (_gm.IsYou(ObjectType.Baba))
        {
            var babas = _board.FindAll(ObjectType.Baba);
            foreach (var pos in babas)
                TryMove(pos, dir);
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

    void TryMove(Vector2Int from, Vector2Int dir)
    {
        Vector2Int to = from + dir;
        if (!_board.InRange(to.x, to.y)) return;

        // 0) 텍스트가 있으면 기본 PUSH로 밀기
        if (HasText(to))
        {
            Vector2Int textTo = to + dir;
            if (!_board.InRange(textTo.x, textTo.y)) return;

            // 최소: 텍스트는 '빈칸'으로만 밀기 (뒤에 오브젝트/텍스트 있으면 막힘)
            if (_board.GetObject(textTo.x, textTo.y) != ObjectType.None) return;
            if (_board.GetText(textTo.x, textTo.y) != TextType.None) return;

            MoveText(to, textTo);
        }

        ObjectType target = _board.GetObject(to.x, to.y);

        // 비어있으면 그냥 이동
        if (target == ObjectType.None)
        {
            _board.SetObject(from.x, from.y, ObjectType.None);
            _board.SetObject(to.x, to.y, ObjectType.Baba);
            CheckWin(to);
            return;
        }

        // STOP이면 막힘
        if (_gm.IsStop(target)) return;

        // PUSH면 한 칸 더 밀 수 있으면 밀고 이동
        if (_gm.IsPush(target))
        {
            Vector2Int pushTo = to + dir;
            if (!_board.InRange(pushTo.x, pushTo.y)) return;

            ObjectType behind = _board.GetObject(pushTo.x, pushTo.y);
            if (behind != ObjectType.None) return; // 최소: 연쇄 PUSH는 다음 단계

            _board.SetObject(pushTo.x, pushTo.y, target); // ROCK 이동
            _board.SetObject(to.x, to.y, ObjectType.Baba);
            _board.SetObject(from.x, from.y, ObjectType.None);
            CheckWin(to);
            return;
        }

        // WIN이면 들어가면 클리어
        if (_gm.IsWin(target))
        {
            _board.SetObject(from.x, from.y, ObjectType.None);
            _board.SetObject(to.x, to.y, ObjectType.Baba);
            Debug.Log("WIN!");
        }
    }

    void CheckWin(Vector2Int babaPos)
    {
        // Baba가 올라간 칸에 Flag가 있으면? (현재 보드는 1칸 1오브젝트라 동시에 못있음)
        // 최소 구현에서는 "들어가는 target이 WIN일 때"만 처리해도 됨.
    }
    bool HasText(Vector2Int p) => _board.GetText(p.x, p.y) != TextType.None;

    void MoveText(Vector2Int from, Vector2Int to)
    {
        var t = _board.GetText(from.x, from.y);
        _board.SetText(from.x, from.y, TextType.None);
        _board.SetText(to.x, to.y, t);
    }
}