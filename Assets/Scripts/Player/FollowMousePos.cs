using UnityEngine;

public class FollowMousePos : MonoBehaviour
{
    [SerializeField] private float _multPos = 1f;

    private void Update()
    {
        transform.position = PlayerInput.MousePositionWorld(transform.position.z) * _multPos;
    }
}
