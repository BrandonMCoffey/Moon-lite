using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [SerializeField] private Transform _player;

    private void LateUpdate()
    {
        var pos = _player.transform.position;
        pos.z = transform.position.z;
        transform.position = pos;
    }
}
