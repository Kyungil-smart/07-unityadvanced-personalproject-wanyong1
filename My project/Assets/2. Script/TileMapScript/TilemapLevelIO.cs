using System.IO;
using UnityEngine;
using static CellType;

public class TilemapLevelIO : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private TilemapBoardManager _board;

    [Header("File")]
    [SerializeField] private string _fileName = "level_01.json";

    private string FullPath => Path.Combine(Application.persistentDataPath, _fileName);

    [ContextMenu("Save Level To JSON")]
    public void Save()
    {
        if (_board == null)
        {
            Debug.LogError("[TilemapLevelIO] Board ref missing.");
            return;
        }

        var data = new LevelData
        {
            width = _board.Width,
            height = _board.Height
        };

        // TilemapBoardManager의 origin은 private이라,
        // 지금 구조에선 JSON에 origin을 '0,0'으로 저장(보드 내부 좌표 기반 저장이라 문제 없음)
        data.origin = Vector2Int.zero;

        // 보드 범위 전체 스캔 -> None 아닌 것만 저장
        for (int y = 0; y < _board.Height; y++)
        {
            for (int x = 0; x < _board.Width; x++)
            {
                var obj = _board.GetObject(x, y);
                if (obj != ObjectType.None)
                    data.objects.Add(new ObjectCell { x = x, y = y, type = obj });

                var txt = _board.GetText(x, y);
                if (txt != TextType.None)
                    data.texts.Add(new TextCell { x = x, y = y, type = txt });
            }
        }

        var json = JsonUtility.ToJson(data, true);
        File.WriteAllText(FullPath, json);

        Debug.Log($"[TilemapLevelIO] Saved: {FullPath}\nObjects={data.objects.Count}, Texts={data.texts.Count}");
    }

    [ContextMenu("Load Level From JSON")]
    public void Load()
    {
        if (_board == null)
        {
            Debug.LogError("[TilemapLevelIO] Board ref missing.");
            return;
        }

        if (!File.Exists(FullPath))
        {
            Debug.LogWarning($"[TilemapLevelIO] File not found: {FullPath}");
            return;
        }

        var json = File.ReadAllText(FullPath);
        var data = JsonUtility.FromJson<LevelData>(json);

        // 1) 기존 보드 비우기 (칸당 1개 구조라 전부 None으로)
        for (int y = 0; y < _board.Height; y++)
        {
            for (int x = 0; x < _board.Width; x++)
            {
                _board.SetObject(x, y, ObjectType.None);
                _board.SetText(x, y, TextType.None);
            }
        }

        // 2) JSON 데이터로 복원
        if (data.objects != null)
        {
            foreach (var c in data.objects)
                _board.SetObject(c.x, c.y, c.type);
        }

        if (data.texts != null)
        {
            foreach (var c in data.texts)
                _board.SetText(c.x, c.y, c.type);
        }

        Debug.Log($"[TilemapLevelIO] Loaded: {FullPath}\nObjects={data.objects?.Count ?? 0}, Texts={data.texts?.Count ?? 0}");
    }

    // 편의용: 에디터에서 빠르게 저장/로드하고 싶으면 사용
    // (Input System 충돌 피하려고 UnityEngine.Input 안 씀)
    public void SaveAs(string fileName)
    {
        _fileName = fileName;
        Save();
    }

    public void LoadFrom(string fileName)
    {
        _fileName = fileName;
        Load();
    }
}