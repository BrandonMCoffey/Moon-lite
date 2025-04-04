using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppedItem : MonoBehaviour
{
    [SerializeField] private float _lifespan;

    private DroppedItemManager _manager;

    public void InitItem(Vector3 position, ItemData data)
    {
        transform.position = position;
    }
}
