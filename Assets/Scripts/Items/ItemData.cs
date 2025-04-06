using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item Data")]
public class ItemData : ScriptableObject
{
    [SerializeField] private Sprite _sprite;
    [SerializeField] private Color _spriteColor = Color.white;

    public Sprite Sprite => _sprite;
    public Color SpriteColor => _spriteColor;
}
