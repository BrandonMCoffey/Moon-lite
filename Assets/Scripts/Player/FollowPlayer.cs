using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [SerializeField] private Transform _moveTransform;
    [SerializeField] private bool _followX = true;
    [SerializeField] private bool _followY = true;
    [SerializeField] private Vector2 _offset = new Vector2(0, 1);

    private Transform _player;

    private void OnValidate()
    {
        if (_moveTransform == null) _moveTransform = transform;
    }

    private void Start()
    {
        _player = Player.Instance.transform;
    }

    private void LateUpdate()
    {
        var pos = _moveTransform.position;
        if (_followX) pos.x = _player.position.x + _offset.x;
        if (_followY) pos.y = _player.position.y + _offset.y;
        _moveTransform.position = pos;
    }
}
