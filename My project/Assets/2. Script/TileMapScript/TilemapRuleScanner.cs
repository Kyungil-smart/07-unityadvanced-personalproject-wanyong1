using UnityEngine;
using static CellType;

public static class TilemapRuleScanner
{
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

    public static RuleSet Build(TilemapBoardManager board)
    {
        var rules = new RuleSet();
        int w = board.Width;
        int h = board.Height;

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                if (board.GetText(x, y) != TextType.Is) continue;

                // 가로: left - IS - right
                TryAdd(board, rules, x - 1, y, x, y, x + 1, y);

                // 세로: down - IS - up
                TryAdd(board, rules, x, y - 1, x, y, x, y + 1);
            }

        return rules;
    }

    private static void TryAdd(TilemapBoardManager board, RuleSet rules,
        int ax, int ay, int bx, int by, int cx, int cy)
    {
        if (!board.InRange(ax, ay) || !board.InRange(cx, cy)) return;

        var a = board.GetText(ax, ay);
        var c = board.GetText(cx, cy);

        if (!IsNoun(a)) return;
        if (!IsProperty(c)) return;

        rules.Add(ToObjectType(a), c);
    }
}