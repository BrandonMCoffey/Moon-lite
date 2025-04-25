using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [SerializeField] private bool _followX = true;
    [SerializeField] private bool _followY = true;
    [SerializeField] private Vector2 _offset = new Vector2(0, 1);

    private Transform _player;

    private void Start()
    {
        _player = Player.Instance.transform;
    }

    private void LateUpdate()
    {
        var pos = transform.position;
        if (_followX) pos.x = _player.position.x + _offset.x;
        if (_followY) pos.y = _player.position.y + _offset.y;
        transform.position = pos;
    }
}
