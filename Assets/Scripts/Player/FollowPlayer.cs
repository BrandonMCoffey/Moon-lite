using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    private Transform _player;

    private void Start()
    {
        _player = Player.Instance.transform;
    }

    private void LateUpdate()
    {
        var pos = _player.transform.position;
        pos.z = transform.position.z;
        transform.position = pos;
    }
}
