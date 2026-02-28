using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static CellType;

public class TilemapBoardManager : MonoBehaviour
{
    [Header("Tilemaps")]
    [SerializeField] private Tilemap _objectTilemap;
    [SerializeField] private Tilemap _textTilemap;

    [Header("Library")]
    [SerializeField] private TileLibrarySO _tiles;

    [Header("Board Size (for scan & bounds)")]
    [SerializeField] private int _width = 12;
    [SerializeField] private int _height = 8;

    // Tilemap의 원점(셀 좌표 시작점). 필요하면 (0,0) 말고도 가능.
    [SerializeField] private Vector2Int _origin = Vector2Int.zero;


    public int Width => _width;
    public int Height => _height;

    public System.Action OnBoardChanged;

    private void Awake()
    {
        if (_objectTilemap == null || _textTilemap == null)
            Debug.LogWarning("[TilemapBoardManager] Tilemap refs missing.");
        if (_tiles == null)
            Debug.LogWarning("[TilemapBoardManager] TileLibrarySO missing.");
    }

    public bool InRange(int x, int y)
        => 0 <= x && x < _width && 0 <= y && y < _height;

    private Vector3Int ToCell(int x, int y)
        => new Vector3Int(_origin.x + x, _origin.y + y, 0);

    public Vector3 GridToWorld(int x, int y)
    {
        // (x,y) 셀의 중심 월드 좌표
        var cell = ToCell(x, y);
        return _objectTilemap.GetCellCenterWorld(cell);
    }

    public bool WorldToGrid(Vector3 worldPos, out int x, out int y)
    {
        var cell = _objectTilemap.WorldToCell(worldPos);
        x = cell.x - _origin.x;
        y = cell.y - _origin.y;
        return InRange(x, y);
    }

    public ObjectType GetObject(int x, int y)
    {
        if (!InRange(x, y)) return ObjectType.None;
        var tile = _objectTilemap.GetTile(ToCell(x, y));
        return (_tiles != null && _tiles.TryGetObjectType(tile, out var t)) ? t : ObjectType.None;
    }

    public TextType GetText(int x, int y)
    {
        if (!InRange(x, y)) return TextType.None;
        var tile = _textTilemap.GetTile(ToCell(x, y));
        return (_tiles != null && _tiles.TryGetTextType(tile, out var t)) ? t : TextType.None;
    }

    public void SetObject(int x, int y, ObjectType type)
    {
        if (!InRange(x, y) || _tiles == null) return;
        var cell = ToCell(x, y);
        _objectTilemap.SetTile(cell, _tiles.GetObjectTile(type));
        OnBoardChanged?.Invoke();
    }

    public void SetText(int x, int y, TextType type)
    {
        if (!InRange(x, y) || _tiles == null) return;
        var cell = ToCell(x, y);
        _textTilemap.SetTile(cell, _tiles.GetTextTile(type));
        OnBoardChanged?.Invoke();
    }

    public List<Vector2Int> FindAll(ObjectType type)
    {
        var list = new List<Vector2Int>();
        for (int y = 0; y < _height; y++)
            for (int x = 0; x < _width; x++)
            {
                if (GetObject(x, y) == type)
                    list.Add(new Vector2Int(x, y));
            }
        return list;
    }

    [ContextMenu("Create Test Level")]
    public void CreateTestLevel()
    {
        // 오브젝트
        SetObject(2, 2, ObjectType.Baba);
        SetObject(4, 2, ObjectType.Rock);
        SetObject(6, 2, ObjectType.Flag);

        // 텍스트: BABA IS YOU
        SetText(1, 6, TextType.Baba);
        SetText(2, 6, TextType.Is);
        SetText(3, 6, TextType.You);

        // FLAG IS WIN
        SetText(6, 6, TextType.Flag);
        SetText(7, 6, TextType.Is);
        SetText(8, 6, TextType.Win);
    }
}