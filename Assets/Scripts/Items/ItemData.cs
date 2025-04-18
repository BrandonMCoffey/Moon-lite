using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Moonlite/Item Data")]
public class ItemData : ScriptableObject
{
    [SerializeField] private string _itemName;
    [SerializeField, TextArea] private string _itemDescription;
    [SerializeField] private Sprite _itemIcon;
    [SerializeField] private Color _itemIconTint = Color.white;

    public Sprite Sprite => _itemIcon;
    public Color SpriteColor => _itemIconTint;
}
