using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static CellType;

public class TilemapBoardManager : MonoBehaviour
{
    [Header("Object Tilemaps (one per type)")]
    [SerializeField] private Tilemap _babaTilemap;
    [SerializeField] private Tilemap _wallTilemap;
    [SerializeField] private Tilemap _rockTilemap;
    [SerializeField] private Tilemap _flagTilemap;
    [SerializeField] private Tilemap _lavaTilemap;
    [SerializeField] private Tilemap _skullTilemap;

    [Header("Text Tilemap")]
    [SerializeField] private Tilemap _textTilemap;

    [Header("Library")]
    [SerializeField] private TileLibrarySO _tiles;

    [Header("Board Size")]
    [SerializeField] private int _width = 12;
    [SerializeField] private int _height = 8;
    [SerializeField] private Vector2Int _origin = Vector2Int.zero;

    public int Width => _width;
    public int Height => _height;

    public Action OnBoardChanged;

    // Cell -> multi objects
    private List<ObjectType>[,] _objs;

    // batch/suppress notify (Object IS Object 변환 같은 "대량 갱신"에서 재귀 RebuildRules 방지)
    private int _suppressNotifyDepth = 0;
    private void NotifyChanged()
    {
        if (_suppressNotifyDepth > 0) return;
        OnBoardChanged?.Invoke();
    }

    public void BeginBatch() => _suppressNotifyDepth++;

    public void EndBatch(bool notify = true)
    {
        _suppressNotifyDepth = Mathf.Max(0, _suppressNotifyDepth - 1);
        if (notify) NotifyChanged();
    }

    private void Awake()
    {
        if (_textTilemap == null) Debug.LogWarning("[TilemapBoardManager] TextTilemap missing.");
        if (_tiles == null) Debug.LogWarning("[TilemapBoardManager] TileLibrarySO missing.");

        _objs = new List<ObjectType>[_width, _height];
        for (int y = 0; y < _height; y++)
            for (int x = 0; x < _width; x++)
                _objs[x, y] = new List<ObjectType>();

        RebuildFromTilemaps();
    }

    public bool InRange(int x, int y) => 0 <= x && x < _width && 0 <= y && y < _height;
    private Vector3Int ToCell(int x, int y) => new Vector3Int(_origin.x + x, _origin.y + y, 0);

    public Vector3 GridToWorld(int x, int y)
    {
        var cell = ToCell(x, y);
        var baseMap = _babaTilemap != null ? _babaTilemap : _textTilemap;
        return baseMap.GetCellCenterWorld(cell);
    }

    public bool WorldToGrid(Vector3 worldPos, out int x, out int y)
    {
        var baseMap = _babaTilemap != null ? _babaTilemap : _textTilemap;
        var cell = baseMap.WorldToCell(worldPos);
        x = cell.x - _origin.x;
        y = cell.y - _origin.y;
        return InRange(x, y);
    }

    // ======================
    // Objects (multi)
    // ======================
    public IReadOnlyList<ObjectType> GetObjects(int x, int y)
        => InRange(x, y) ? _objs[x, y] : Array.Empty<ObjectType>();

    public void AddObject(int x, int y, ObjectType type)
    {
        if (!InRange(x, y) || type == ObjectType.None) return;

        _objs[x, y].Add(type);
        RenderObjectPresence(x, y, type, present: true);
        NotifyChanged();
    }

    public bool RemoveObjectOnce(int x, int y, ObjectType type)
    {
        if (!InRange(x, y)) return false;

        var list = _objs[x, y];
        int idx = list.IndexOf(type);
        if (idx < 0) return false;

        list.RemoveAt(idx);

        // 같은 타입이 더 남아있으면 타일 유지
        bool still = list.Contains(type);
        RenderObjectPresence(x, y, type, present: still);

        NotifyChanged();
        return true;
    }

    public void MoveObjectOnce(int fromX, int fromY, int toX, int toY, ObjectType type)
    {
        if (!RemoveObjectOnce(fromX, fromY, type)) return;
        AddObject(toX, toY, type);
    }

    public List<Vector2Int> FindAll(ObjectType type)
    {
        var result = new List<Vector2Int>();
        for (int y = 0; y < _height; y++)
            for (int x = 0; x < _width; x++)
            {
                int count = 0;
                var list = _objs[x, y];
                for (int i = 0; i < list.Count; i++)
                    if (list[i] == type) count++;

                for (int k = 0; k < count; k++)
                    result.Add(new Vector2Int(x, y));
            }
        return result;
    }

    // ======================
    // Text (single)
    // ======================
    public TextType GetText(int x, int y)
    {
        if (!InRange(x, y) || _tiles == null || _textTilemap == null) return TextType.None;
        var tile = _textTilemap.GetTile(ToCell(x, y));
        return _tiles.TryGetTextType(tile, out var t) ? t : TextType.None;
    }

    public void SetText(int x, int y, TextType type)
    {
        if (!InRange(x, y) || _tiles == null || _textTilemap == null) return;
        _textTilemap.SetTile(ToCell(x, y), _tiles.GetTextTile(type));
        NotifyChanged();
    }

    // ======================
    // Build from painted tilemaps
    // ======================
    [ContextMenu("Rebuild From Tilemaps")]
    public void RebuildFromTilemaps()
    {
        BeginBatch();

        for (int y = 0; y < _height; y++)
            for (int x = 0; x < _width; x++)
                _objs[x, y].Clear();

        LoadTypeFromTilemap(_babaTilemap, ObjectType.Baba);
        LoadTypeFromTilemap(_wallTilemap, ObjectType.Wall);
        LoadTypeFromTilemap(_rockTilemap, ObjectType.Rock);
        LoadTypeFromTilemap(_flagTilemap, ObjectType.Flag);
        LoadTypeFromTilemap(_lavaTilemap, ObjectType.Lava);
        LoadTypeFromTilemap(_skullTilemap, ObjectType.Skull);

        EndBatch(notify: true);
    }

    private void LoadTypeFromTilemap(Tilemap map, ObjectType type)
    {
        if (map == null) return;

        for (int y = 0; y < _height; y++)
            for (int x = 0; x < _width; x++)
            {
                var t = map.GetTile(ToCell(x, y));
                if (t != null)
                    _objs[x, y].Add(type);
            }
    }

    private void RenderObjectPresence(int x, int y, ObjectType type, bool present)
    {
        if (_tiles == null) return;

        var cell = ToCell(x, y);
        Tilemap map = type switch
        {
            ObjectType.Baba => _babaTilemap,
            ObjectType.Wall => _wallTilemap,
            ObjectType.Rock => _rockTilemap,
            ObjectType.Flag => _flagTilemap,
            ObjectType.Lava => _lavaTilemap,
            ObjectType.Skull => _skullTilemap,
            _ => null
        };
        if (map == null) return;

        map.SetTile(cell, present ? _tiles.GetObjectTile(type) : null);
    }
    public void ClearAllTilesFast()
    {
        if (_babaTilemap != null) _babaTilemap.ClearAllTiles();
        if (_wallTilemap != null) _wallTilemap.ClearAllTiles();
        if (_rockTilemap != null) _rockTilemap.ClearAllTiles();
        if (_flagTilemap != null) _flagTilemap.ClearAllTiles();
        if (_lavaTilemap != null) _lavaTilemap.ClearAllTiles();
        if (_skullTilemap != null) _skullTilemap.ClearAllTiles();
        if (_textTilemap != null) _textTilemap.ClearAllTiles();
    }

    public void ClearAllLogicalFast()
    {
        for (int y = 0; y < _height; y++)
            for (int x = 0; x < _width; x++)
                _objs[x, y].Clear();
    }
}