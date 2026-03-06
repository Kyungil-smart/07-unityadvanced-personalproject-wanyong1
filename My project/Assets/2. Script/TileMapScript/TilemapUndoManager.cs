using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static CellType;

public class TilemapUndoManager : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private TilemapBoardManager _board;

    [Header("Undo")]
    [SerializeField, Min(1)] private int _maxHistory = 200;

    private readonly Stack<LevelData> _history = new();

    // move 한 번 처리 전/후 변화 감지용
    private LevelData _pendingSnapshot;
    private int _pendingHash;

    public event Action OnUndone;

    private void Awake()
    {
        if (_board == null) _board = FindFirstObjectByType<TilemapBoardManager>();
    }

    private void Update()
    {
        // Z: Undo
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Undo();
        }

        // R: Restart Scene
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartScene();
        }
    }

    // TilemapPlayerInputMover에서 "방향 입력이 들어와 실제 이동을 시도하기 직전" 호출
    public void BeginMove()
    {
        if (_board == null) return;

        _pendingSnapshot = CaptureState();
        _pendingHash = ComputeStateHash();
    }

    // TilemapPlayerInputMover에서 "이동 처리가 끝난 뒤" 호출
    public void EndMove()
    {
        if (_board == null) return;

        // 변화가 없으면(막혀서 못 움직였으면) 스택에 안 쌓음
        int afterHash = ComputeStateHash();
        if (afterHash == _pendingHash) return;

        PushHistory(_pendingSnapshot);
        _pendingSnapshot = null;
    }

    public void Undo()
    {
        if (_board == null) return;
        if (_history.Count == 0) return;

        // 클리어 UI에서 TimeScale 0 해놓은 상태여도 Undo가 되도록 복구
        Time.timeScale = 1f;

        var prev = _history.Pop();
        RestoreState(prev);

        // HasWon은 보드 변경 이벤트로 RebuildRules 되면서 false로 돌아감(현재 구조상) :contentReference[oaicite:1]{index=1}
        OnUndone?.Invoke();
    }

    public void RestartScene()
    {
        Time.timeScale = 1f;
        var active = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(active);
    }

    private void PushHistory(LevelData snapshot)
    {
        if (snapshot == null) return;

        // maxHistory 제한: 오래된 것 버리기(스택이라 쉽게 못 버리니 리스트로 재구성)
        if (_history.Count >= _maxHistory)
        {
            var list = new List<LevelData>(_history);
            list.Reverse(); // oldest -> newest
            // oldest 하나 제거
            list.RemoveAt(0);
            _history.Clear();
            // 다시 push (oldest -> newest)
            for (int i = 0; i < list.Count; i++)
                _history.Push(list[i]);
        }

        _history.Push(snapshot);
    }

    // ====== State Capture / Restore ======

    private LevelData CaptureState()
    {
        var data = new LevelData
        {
            width = _board.Width,
            height = _board.Height,
            origin = Vector2Int.zero
        };

        for (int y = 0; y < _board.Height; y++)
        {
            for (int x = 0; x < _board.Width; x++)
            {
                var objs = _board.GetObjects(x, y);
                for (int i = 0; i < objs.Count; i++)
                {
                    var o = objs[i];
                    if (o != ObjectType.None)
                        data.objects.Add(new ObjectEntry { x = x, y = y, type = o });
                }

                var t = _board.GetText(x, y);
                if (t != TextType.None)
                    data.texts.Add(new TextCell { x = x, y = y, type = t });
            }
        }

        return data;
    }

    private void RestoreState(LevelData data)
    {
        if (data == null) return;

        _board.BeginBatch();

        // 타일맵들 싹 비우기 (렌더)
        _board.ClearAllTilesFast();

        // 논리 데이터 싹 비우기
        _board.ClearAllLogicalFast();

        // 복원
        if (data.objects != null)
        {
            foreach (var e in data.objects)
                _board.AddObject(e.x, e.y, e.type);
        }

        if (data.texts != null)
        {
            foreach (var c in data.texts)
                _board.SetText(c.x, c.y, c.type);
        }

        _board.EndBatch(notify: true);
    }

    private int ComputeStateHash()
    {
        unchecked
        {
            int h = 17;
            h = h * 31 + _board.Width;
            h = h * 31 + _board.Height;

            for (int y = 0; y < _board.Height; y++)
            {
                for (int x = 0; x < _board.Width; x++)
                {
                    // text
                    h = h * 31 + (int)_board.GetText(x, y);

                    var objs = _board.GetObjects(x, y);
                    // 타입별 카운트(순서 무관)
                    // ObjectType enum 수가 작아서 간단히 누적
                    int cell = 0;
                    for (int i = 0; i < objs.Count; i++)
                        cell = cell * 37 + (int)objs[i];

                    h = h * 31 + cell;
                }
            }

            return h;
        }
    }
}