using System;
using System.IO;
using UnityEngine;

[Serializable]
public class StageProgress
{
    public int version = 1;
    public bool[] cleared;
}

public static class StageProgressIO
{
    private const string FileName = "stage_progress.json";
    private static string PathFull => System.IO.Path.Combine(Application.persistentDataPath, FileName);

    public static StageProgress LoadOrCreate(int stageCount)
    {
        if (File.Exists(PathFull))
        {
            var json = File.ReadAllText(PathFull);
            var data = JsonUtility.FromJson<StageProgress>(json);

            // 스테이지 개수 늘어났을 때 확장
            if (data.cleared == null) data.cleared = new bool[stageCount];
            if (data.cleared.Length != stageCount)
            {
                var old = data.cleared;
                data.cleared = new bool[stageCount];
                int n = Mathf.Min(old.Length, stageCount);
                for (int i = 0; i < n; i++) data.cleared[i] = old[i];
            }
            return data;
        }

        return new StageProgress { cleared = new bool[stageCount] };
    }

    public static void Save(StageProgress data)
    {
        var json = JsonUtility.ToJson(data, true);
        File.WriteAllText(PathFull, json);
    }

    public static void MarkCleared(int stageIndex0Based, int stageCount)
    {
        var data = LoadOrCreate(stageCount);
        if (0 <= stageIndex0Based && stageIndex0Based < data.cleared.Length)
        {
            data.cleared[stageIndex0Based] = true;
            Save(data);
        }
    }
}