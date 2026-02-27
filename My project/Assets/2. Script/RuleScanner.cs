using UnityEngine;
using static CellType;

public static class RuleScanner
{
    // 인식할 속성들(최소)
    private static bool IsProperty(TextType t)
        => t == TextType.You || t == TextType.Push || t == TextType.Stop || t == TextType.Win;

    private static bool IsNoun(TextType t)
        => t == TextType.Baba || t == TextType.Rock || t == TextType.Wall || t == TextType.Flag;

    private static ObjectType ToObjectType(TextType noun)
    {
        return noun switch
        {
            TextType.Baba => ObjectType.Baba,
            TextType.Rock => ObjectType.Rock,
            TextType.Wall => ObjectType.Wall,
            TextType.Flag => ObjectType.Flag,
            _ => ObjectType.None
        };
    }

    public static RuleSet Build(BoardManager board)
    {
        var rules = new RuleSet();

        int w = board.Width;
        int h = board.Height;

        // 가로/세로만
        Vector2Int[] dirs = { Vector2Int.right, Vector2Int.up };

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                foreach (var dir in dirs)
                {
                    int x2 = x + dir.x;
                    int y2 = y + dir.y;
                    int x3 = x + dir.x * 2;
                    int y3 = y + dir.y * 2;

                    if (!board.InRange(x2, y2) || !board.InRange(x3, y3))
                        continue;

                    TextType a = board.GetText(x, y);
                    TextType b = board.GetText(x2, y2);
                    TextType c = board.GetText(x3, y3);

                    // [Noun] [Is] [Property]
                    if (IsNoun(a) && b == TextType.Is && IsProperty(c))
                    {
                        rules.Add(ToObjectType(a), c);
                    }
                }
            }

        return rules;
    }
}