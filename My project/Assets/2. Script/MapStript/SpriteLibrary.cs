using System;
using UnityEngine;
using static CellType;

public class SpriteLibrary : MonoBehaviour
{
    [Serializable]
    public struct ObjectSpritePair
    {
        public ObjectType type;
        public Sprite sprite;
    }

    [Serializable]
    public struct TextSpritePair
    {
        public TextType type;
        public Sprite sprite;
    }

    [Header("Object Sprites")]
    [SerializeField] private ObjectSpritePair[] _objectSprites;

    [Header("Text Sprites")]
    [SerializeField] private TextSpritePair[] _textSprites;

    public Sprite GetObjectSprite(ObjectType type)
    {
        for (int i = 0; i < _objectSprites.Length; i++)
            if (_objectSprites[i].type == type) return _objectSprites[i].sprite;
        return null;
    }

    public Sprite GetTextSprite(TextType type)
    {
        for (int i = 0; i < _textSprites.Length; i++)
            if (_textSprites[i].type == type) return _textSprites[i].sprite;
        return null;
    }
}