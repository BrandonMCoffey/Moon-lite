using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemVisual : MonoBehaviour
{
    [SerializeField] private ItemData _data;
    [SerializeField] private int _count;

    [Header("References")]
    [SerializeField] private Image _iconImage;
    [SerializeField] private TMP_Text _countText;

    public void UpdateItemData(ItemData data)
    {
        _data = data;
        _iconImage.sprite = data.Sprite;
        _count = 0;
    }

    public void UpdateCount(int count) => _count = count;
}
