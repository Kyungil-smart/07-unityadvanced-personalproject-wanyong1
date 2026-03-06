// 쓸모 없어졌다. Json으로 맵 레벨 저장/불러오기 하는 기능


//using System.IO;
//using UnityEngine;
//using static CellType;

//public class TilemapLevelIO : MonoBehaviour
//{
//    [Header("Refs")]
//    [SerializeField] private TilemapBoardManager _board;

//    [Header("File")]
//    [SerializeField] private string _fileName = "level_01.json";

//    private string FullPath => Path.Combine(Application.persistentDataPath, _fileName);

//    [ContextMenu("Save Level To JSON")]
//    public void Save()
//    {
//        if (_board == null)
//        {
//            Debug.LogError("[TilemapLevelIO] Board ref missing.");
//            return;
//        }

//        var data = new LevelData
//        {
//            width = _board.Width,
//            height = _board.Height,
//            origin = Vector2Int.zero
//        };

//        멀티 오브젝트: 각 칸의 리스트를 전부 저장
//        for (int y = 0; y < _board.Height; y++)
//        {
//            for (int x = 0; x < _board.Width; x++)
//            {
//                var objs = _board.GetObjects(x, y);
//                for (int i = 0; i < objs.Count; i++)
//                {
//                    var obj = objs[i];
//                    if (obj != ObjectType.None)
//                        data.objects.Add(new ObjectEntry { x = x, y = y, type = obj });
//                }

//                var txt = _board.GetText(x, y);
//                if (txt != TextType.None)
//                    data.texts.Add(new TextCell { x = x, y = y, type = txt });
//            }
//        }

//        var json = JsonUtility.ToJson(data, true);
//        File.WriteAllText(FullPath, json);

//        Debug.Log($"[TilemapLevelIO] Saved: {FullPath}\nObjects={data.objects.Count}, Texts={data.texts.Count}");
//    }

//    [ContextMenu("Load Level From JSON")]
//    public void Load()
//    {
//        if (_board == null)
//        {
//            Debug.LogError("[TilemapLevelIO] Board ref missing.");
//            return;
//        }

//        if (!File.Exists(FullPath))
//        {
//            Debug.LogWarning($"[TilemapLevelIO] File not found: {FullPath}");
//            return;
//        }

//        var json = File.ReadAllText(FullPath);
//        var data = JsonUtility.FromJson<LevelData>(json);

//        for (int y = 0; y < _board.Height; y++)
//        {
//            for (int x = 0; x < _board.Width; x++)
//            {
//                var objs = _board.GetObjects(x, y);
//                for (int i = objs.Count - 1; i >= 0; i--)
//                {
//                    _board.RemoveObjectOnce(x, y, objs[i]);
//                }

//                _board.SetText(x, y, TextType.None);
//            }
//        }

//        2) JSON 복원
//        if (data.objects != null)
//        {
//            foreach (var e in data.objects)
//                _board.AddObject(e.x, e.y, e.type);
//        }

//        if (data.texts != null)
//        {
//            foreach (var c in data.texts)
//                _board.SetText(c.x, c.y, c.type);
//        }

//        Debug.Log($"[TilemapLevelIO] Loaded: {FullPath}\nObjects={data.objects?.Count ?? 0}, Texts={data.texts?.Count ?? 0}");
//    }

//    public void SaveAs(string fileName)
//    {
//        _fileName = fileName;
//        Save();
//    }

//    public void LoadFrom(string fileName)
//    {
//        _fileName = fileName;
//        Load();
//    }
//}