using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.LookDev;
using UnityEngine;

public class CameraShakeManager : MonoBehaviour
{
    // TODO: Switch this out with Cinemachine?

    [SerializeField] private Transform _cameraTransform;

    private static bool _shake;
    private static float _shakeTimeMax;
    private static float _shakeTimer;
    private static float _shakeMagnitude;

    private Vector3 _originalPos;

    private void OnValidate()
    {
        if (_cameraTransform == null) _cameraTransform = transform;
    }

    private void Awake()
    {
        _originalPos = transform.localPosition;
    }

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

            transform.localPosition = _originalPos + Random.insideUnitSphere * (_shakeMagnitude * delta * 0.5f);
        }
        else if (_shake)
        {
            transform.localPosition = _originalPos;
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
