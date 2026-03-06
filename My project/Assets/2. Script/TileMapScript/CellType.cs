using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CellType
{
    public enum CellLayer
    {
        objcet, Text
    }

    public enum ObjectType
    {
        None,
        Baba, Wall, Rock, Flag, Lava, Skull
    }
    public enum TextType
    {
        None,
        Baba, Rock, Wall, Flag,
        You, Push, Stop, Win, Is,
        Hot, Defeat, Melt, Lava, Skull
    }
}
