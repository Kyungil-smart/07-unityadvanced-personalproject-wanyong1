using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellType : MonoBehaviour
{
    public enum CellLayer
    {
        objcet, Text
    }

    public enum ObjectType
    {
        None, 
        Baba,Wall,Rock,Flag,
    }
    public enum TextType
    {
        None,
        Baba,Rock,Wall,Flag,
        You,Push,Stop,Win,Is
    }
}
