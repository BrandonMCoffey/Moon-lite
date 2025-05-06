using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.LookDev;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static Camera PlayerCamera;

    private static bool _shake;
    private static float _shakeTimeMax;
    private static float _shakeTimer;
    private static float _shakeMagnitude;

    private Transform _cameraTransform;

    // NOTE: This assume that the camera is NOT being moved by any other script, moving a parent is fine but not the camera directly.
    private Vector3 _originalPos;

    private void Awake()
    {
        PlayerCamera = GetComponent<Camera>();
        if (PlayerCamera == null) PlayerCamera = GetComponentInChildren<Camera>();
        _cameraTransform = PlayerCamera.transform;
        _originalPos = _cameraTransform.localPosition;
    }

    // TODO: Switch camera shake with Cinemachine?

    private void Update()
    {
        if (!_shake) return;

        if (_shakeTimer > 0)
        {
            _shakeTimer -= Time.deltaTime;

            // Ease in out camera shake
            float delta = 1f - (2f * _shakeTimer / _shakeTimeMax);
            if (delta < 0) delta = -delta;
            delta = 1 - (1 - delta) * (1 - delta);

            _cameraTransform.localPosition = _originalPos + Random.insideUnitSphere * (_shakeMagnitude * delta * 0.1f);
        }
        else if (_shake)
        {
            _cameraTransform.localPosition = _originalPos;
        }
    }

    public static void Shake(float duration, float amount)
    {
        _shakeTimer = Mathf.Max(_shakeTimer, duration);
        _shakeTimeMax = _shakeTimer;
        _shakeMagnitude = amount;
        _shake = true;
    }
}
