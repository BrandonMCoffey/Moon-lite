using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [SerializeField] private Transform _player;

    private void Start()
    {
        if (_player == null) _player = Player.Instance.transform;
    }

    private void LateUpdate()
    {
        var pos = _player.transform.position;
        pos.z = transform.position.z;
        transform.position = pos;
    }
}
