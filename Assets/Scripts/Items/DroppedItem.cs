using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppedItem : MonoBehaviour
{
    [SerializeField] private float _lifespan;
    [SerializeField] private SpriteRenderer _renderer;

    private DroppedItemManager _manager;

    private void OnValidate()
    {
        if (_renderer == null) _renderer = GetComponent<SpriteRenderer>();
    }

    public void InitItem(Vector3 position, ItemData data)
    {
        transform.position = position;
        _renderer.sprite = data.Sprite;
        _renderer.color = data.SpriteColor;
    }
}
