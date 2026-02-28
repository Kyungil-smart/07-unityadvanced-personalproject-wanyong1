using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using static CellType;

[CreateAssetMenu(menuName = "BabaClone/Tile Library", fileName = "TileLibrary")]
public class TileLibrarySO : ScriptableObject
{
    [Serializable]
    public struct ObjectTilePair
    {
        public ObjectType type;
        public TileBase tile;
    }

    [Serializable]
    public struct TextTilePair
    {
        public TextType type;
        public TileBase tile;
    }

    [Header("Object Tiles")]
    [SerializeField] private ObjectTilePair[] _objectTiles;

    [Header("Text Tiles")]
    [SerializeField] private TextTilePair[] _textTiles;

    public TileBase GetObjectTile(ObjectType type)
    {
        for (int i = 0; i < _objectTiles.Length; i++)
            if (_objectTiles[i].type == type) return _objectTiles[i].tile;
        return null;
    }

    public TileBase GetTextTile(TextType type)
    {
        for (int i = 0; i < _textTiles.Length; i++)
            if (_textTiles[i].type == type) return _textTiles[i].tile;
        return null;
    }

    public bool TryGetObjectType(TileBase tile, out ObjectType type)
    {
        for (int i = 0; i < _objectTiles.Length; i++)
        {
            if (_objectTiles[i].tile == tile)
            {
                type = _objectTiles[i].type;
                return true;
            }
        }
        type = ObjectType.None;
        return false;
    }

    public bool TryGetTextType(TileBase tile, out TextType type)
    {
        for (int i = 0; i < _textTiles.Length; i++)
        {
            if (_textTiles[i].tile == tile)
            {
                type = _textTiles[i].type;
                return true;
            }
        }
        type = TextType.None;
        return false;
    }
}