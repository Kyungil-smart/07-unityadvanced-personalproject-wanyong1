using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer objectSprite;

    public void SetSprite(Sprite sprite)
    {
        if(objectSprite == null)
        {
            objectSprite = GetComponent<SpriteRenderer>();
        }
        objectSprite.sprite = sprite;
        objectSprite.enabled = sprite != null;
    }
}
