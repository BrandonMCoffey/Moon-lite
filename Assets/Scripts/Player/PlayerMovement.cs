using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _speed;
    [SerializeField] private float _jumpHeight = 2;
    [SerializeField] private Rigidbody _rb;

    private void OnValidate()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (Time.timeScale == 0) return;
        float moveHorizontal = Input.GetAxis("Horizontal");
        bool jump = Input.GetKeyDown(KeyCode.Space);

        float currHorz = _rb.velocity.x;
        float goalHorz = moveHorizontal * _speed;
        _rb.AddForce(new Vector3(goalHorz - currHorz, 0, 0));

        if (jump)
        {
            var v = _rb.velocity;
            v.y = Mathf.Sqrt(2 * _jumpHeight * Physics.gravity.magnitude);
            _rb.velocity = v;
        }
    }
}
