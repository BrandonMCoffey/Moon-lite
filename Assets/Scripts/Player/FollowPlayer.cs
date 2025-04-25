using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [SerializeField] private bool _followX = true;
    [SerializeField] private bool _followY = true;

    private Transform _player;

    private void Start()
    {
        _player = Player.Instance.transform;
    }

    private void LateUpdate()
    {
        var pos = transform.position;
        if (_followX) pos.x = _player.position.x;
        if (_followY) pos.y = _player.position.y;
        transform.position = pos;
    }
}
