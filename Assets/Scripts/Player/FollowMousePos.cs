using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowMousePos : MonoBehaviour
{
    private void Update()
    {
        transform.position = PlayerInput.MousePositionWorld(transform.position.z);
    }
}
