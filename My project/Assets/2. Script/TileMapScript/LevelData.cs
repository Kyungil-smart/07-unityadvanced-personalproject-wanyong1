using System;
using System.Collections.Generic;
using UnityEngine;
using static CellType;

[Serializable]
public class LevelData
{
    public int version = 1;

    // TilemapBoardManager의 보드 세팅도 같이 저장
    public int width;
    public int height;
    public Vector2Int origin;

    // 스파스 저장(타일이 있는 칸만 저장)
    public List<ObjectCell> objects = new();
    public List<TextCell> texts = new();
}

[Serializable]
public struct ObjectCell
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