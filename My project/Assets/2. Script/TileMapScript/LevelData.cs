using System;
using System.Collections.Generic;
using UnityEngine;
using static CellType;

[Serializable]
public class LevelData
{
    public int version = 2;
    public int width;
    public int height;
    public Vector2Int origin;

    // 멀티 오브젝트 저장: 같은 칸에 여러 개면 여러 개 들어감
    public List<ObjectEntry> objects = new();
    public List<TextCell> texts = new();
}

[Serializable]
public struct ObjectEntry
{
    public int x, y;
    public ObjectType type;
}

[Serializable]
public struct TextCell
{
    public int x, y;
    public TextType type;
}