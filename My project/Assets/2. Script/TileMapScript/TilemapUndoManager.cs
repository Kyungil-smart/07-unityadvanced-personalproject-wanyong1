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

    public void BeginMove()
    {
        if (_board == null) return;

        _pendingSnapshot = CaptureState();
        _pendingHash = ComputeStateHash();
    }

    public void EndMove()
    {
        if (_board == null) return;

        int afterHash = ComputeStateHash();
        if (afterHash == _pendingHash) return;

        PushHistory(_pendingSnapshot);
        _pendingSnapshot = null;
    }

    public void Undo()
    {
        if (_board == null) return;
        if (_history.Count == 0) return;

        Time.timeScale = 1f;

        var prev = _history.Pop();
        RestoreState(prev);

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

        
        if (_history.Count >= _maxHistory)
        {
            var list = new List<LevelData>(_history);
            list.Reverse();
            list.RemoveAt(0);
            _history.Clear();
            for (int i = 0; i < list.Count; i++)
                _history.Push(list[i]);
        }

        _history.Push(snapshot);
    }


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

        _board.ClearAllTilesFast();

        _board.ClearAllLogicalFast();

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
                    h = h * 31 + (int)_board.GetText(x, y);

                    var objs = _board.GetObjects(x, y);

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