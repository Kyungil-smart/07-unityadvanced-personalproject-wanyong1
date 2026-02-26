using UnityEngine;
using static CellType;

public class BoardEditor : MonoBehaviour
{
    public enum EditLayer { Object, Text }

    [Header("Refs")]
    [SerializeField] private BoardManager _board;

    [Header("Edit State")]
    [SerializeField] private EditLayer _layer = EditLayer.Object;
    [SerializeField] private ObjectType _selectedObject = ObjectType.Baba;
    [SerializeField] private TextType _selectedText = TextType.Baba;

    private Camera _cam;

    private void Awake()
    {
        _cam = Camera.main;
    }

    private void Update()
    {
        HandleHotkeys();
        HandleMousePaint();
    }

    void HandleHotkeys()
    {
        // 레이어 전환
        if (Input.GetKeyDown(KeyCode.Tab))
            _layer = (_layer == EditLayer.Object) ? EditLayer.Text : EditLayer.Object;

        // Object 선택(1~4)
        if (Input.GetKeyDown(KeyCode.Alpha1)) { _layer = EditLayer.Object; _selectedObject = ObjectType.Baba; }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { _layer = EditLayer.Object; _selectedObject = ObjectType.Rock; }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { _layer = EditLayer.Object; _selectedObject = ObjectType.Wall; }
        if (Input.GetKeyDown(KeyCode.Alpha4)) { _layer = EditLayer.Object; _selectedObject = ObjectType.Flag; }

        // Text 선택(Q~O)
        if (Input.GetKeyDown(KeyCode.Q)) { _layer = EditLayer.Text; _selectedText = TextType.Baba; }
        if (Input.GetKeyDown(KeyCode.W)) { _layer = EditLayer.Text; _selectedText = TextType.Rock; }
        if (Input.GetKeyDown(KeyCode.E)) { _layer = EditLayer.Text; _selectedText = TextType.Wall; }
        if (Input.GetKeyDown(KeyCode.R)) { _layer = EditLayer.Text; _selectedText = TextType.Flag; }

        if (Input.GetKeyDown(KeyCode.A)) { _layer = EditLayer.Text; _selectedText = TextType.Is; }
        if (Input.GetKeyDown(KeyCode.S)) { _layer = EditLayer.Text; _selectedText = TextType.You; }
        if (Input.GetKeyDown(KeyCode.D)) { _layer = EditLayer.Text; _selectedText = TextType.Win; }
        if (Input.GetKeyDown(KeyCode.F)) { _layer = EditLayer.Text; _selectedText = TextType.Stop; }
        if (Input.GetKeyDown(KeyCode.G)) { _layer = EditLayer.Text; _selectedText = TextType.Push; }
    }

    void HandleMousePaint()
    {
        if (_cam == null || _board == null) return;

        // 왼클릭: 배치 / 우클릭: 삭제
        bool paint = Input.GetMouseButton(0);
        bool erase = Input.GetMouseButton(1);
        if (!paint && !erase) return;

        Vector3 world = _cam.ScreenToWorldPoint(Input.mousePosition);
        world.z = 0f;

        if (!_board.WorldToGrid(world, out int gx, out int gy)) return;

        if (erase)
        {
            if (_layer == EditLayer.Object) _board.SetObject(gx, gy, ObjectType.None);
            else _board.SetText(gx, gy, TextType.None);
        }
        else
        {
            if (_layer == EditLayer.Object) _board.SetObject(gx, gy, _selectedObject);
            else _board.SetText(gx, gy, _selectedText);
        }
    }
}