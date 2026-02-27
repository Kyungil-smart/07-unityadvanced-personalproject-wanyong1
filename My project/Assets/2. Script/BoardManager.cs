using UnityEngine;
using static CellType;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    [Header("Refs")]

    [SerializeField] private CellView _cellViewPrefab;
    [SerializeField] private SpriteLibrary _spriteLibrary;

    private ObjectType[,] _objects;
    private TextType[,] _texts;

    private CellView[,] _objectViews;
    private CellView[,] _textViews;

    [Header("Gizmos")]
    [SerializeField] private bool _drawGizmos = false;     // GridSystem에서 그리면 여기 꺼도 됨
    [SerializeField] private bool _drawGridLines = true;

    [SerializeField] private int _width = 12;
    [SerializeField] private int _height = 8;

    public int Width => _width;
    public int Height => _height;

    public System.Action OnBoardChanged;

    [SerializeField] private float _cellSize = 1f;

    private void Awake()
    {


        InitData();
        CreateViews();

        // 테스트 레벨 (원하면 지워도 됨)
        CreateTestLevel();
        RenderAll();
    }

    void InitData()
    {
        _objects = new ObjectType[_width, _height];
        _texts = new TextType[_width, _height];

        _objectViews = new CellView[_width, _height];
        _textViews = new CellView[_width, _height];
    }

    void CreateViews()
    {
        var objRoot = new GameObject("ObjectViews").transform;
        objRoot.SetParent(transform);

        var textRoot = new GameObject("TextViews").transform;
        textRoot.SetParent(transform);

        int w = _width;
        int h = _height;

        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                Vector3 pos = GridToWorld(x, y);

                var ov = Instantiate(_cellViewPrefab, pos, Quaternion.identity, objRoot);
                ov.name = $"O({x},{y})";
                _objectViews[x, y] = ov;

                // Text는 살짝 앞으로(Z-) 또는 Sorting Order로 위에 보이게
                var tv = Instantiate(_cellViewPrefab, pos + new Vector3(0, 0, -0.1f), Quaternion.identity, textRoot);
                tv.name = $"T({x},{y})";
                _textViews[x, y] = tv;
            }
    }

    public void RenderAll()
    {
        int w = _width;
        int h = _height;

        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                var o = _objects[x, y];
                _objectViews[x, y].SetSprite(o == ObjectType.None ? null : _spriteLibrary.GetObjectSprite(o));

                var t = _texts[x, y];
                _textViews[x, y].SetSprite(t == TextType.None ? null : _spriteLibrary.GetTextSprite(t));
            }
    }

    public void SetObject(int x, int y, ObjectType type)
    {
        if (!InRange(x, y)) return;
        _objects[x, y] = type;
        _objectViews[x, y].SetSprite(type == ObjectType.None ? null : _spriteLibrary.GetObjectSprite(type));
        OnBoardChanged?.Invoke();
    }

    public void SetText(int x, int y, TextType type)
    {
        if (!InRange(x, y)) return;
        _texts[x, y] = type;
        _textViews[x, y].SetSprite(type == TextType.None ? null : _spriteLibrary.GetTextSprite(type));
        OnBoardChanged?.Invoke();
    }

    public ObjectType GetObject(int x, int y) => _objects[x, y];
    public TextType GetText(int x, int y) => _texts[x, y];

    public bool InRange(int x, int y) => 0 <= x && x < _width && 0 <= y && y < _height;

    public Vector3 GridToWorld(int x, int y)
    {
        return transform.position + new Vector3(x * _cellSize, y * _cellSize, 0f);
    }

    public bool WorldToGrid(Vector3 worldPos, out int x, out int y)
    {
        Vector3 local = worldPos - transform.position;
        x = Mathf.RoundToInt(local.x / _cellSize);
        y = Mathf.RoundToInt(local.y / _cellSize);
        return InRange(x, y);
    }
    public List<Vector2Int> FindAll(ObjectType type)
    {
        var list = new List<Vector2Int>();
        for (int y = 0; y < _height; y++)
            for (int x = 0; x < _width; x++)
            {
                if (_objects[x, y] == type)
                    list.Add(new Vector2Int(x, y));
            }
        return list;
    }

    void CreateTestLevel()
    {
        // 오브젝트
        SetObject(2, 2, ObjectType.Baba);
        SetObject(4, 2, ObjectType.Rock);
        SetObject(6, 2, ObjectType.Flag);

        // 텍스트: BABA IS YOU
        // ※ 네 프로젝트 enum 네이밍(TextType.Baba/Is/You vs TextType.BABA/IS/YOU)에 맞춰 사용 중
        SetText(1, 6, TextType.Baba);
        SetText(2, 6, TextType.Is);
        SetText(3, 6, TextType.You);

        // FLAG IS WIN
        SetText(6, 6, TextType.Flag);
        SetText(7, 6, TextType.Is);
        SetText(8, 6, TextType.Win);
    }

    private void OnDrawGizmos()
    {
        if (!_drawGizmos) return;

        

        float cs = Mathf.Max(0.01f, _cellSize);
        int w = Mathf.Max(1, _width);
        int h = Mathf.Max(1, _height);

        Vector3 origin = transform.position;
        float boardW = w * cs;
        float boardH = h * cs;

        Gizmos.color = Color.white;
        Vector3 center = origin + new Vector3(boardW * 0.5f, boardH * 0.5f, 0f);
        Gizmos.DrawWireCube(center, new Vector3(boardW, boardH, 0f));

        if (_drawGridLines)
        {
            Gizmos.color = new Color(1f, 1f, 1f, 0.35f);

            for (int x = 0; x <= w; x++)
            {
                float px = origin.x + x * cs;
                Gizmos.DrawLine(new Vector3(px, origin.y, 0f), new Vector3(px, origin.y + boardH, 0f));
            }

            for (int y = 0; y <= h; y++)
            {
                float py = origin.y + y * cs;
                Gizmos.DrawLine(new Vector3(origin.x, py, 0f), new Vector3(origin.x + boardW, py, 0f));
            }
        }
    }
}
