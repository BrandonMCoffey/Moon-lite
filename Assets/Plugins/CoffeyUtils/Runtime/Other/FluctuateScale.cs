using CoffeyUtils;
using UnityEngine;

public class FluctuateScale : MonoBehaviour
{
    [SerializeField] private Transform _scaleObject;
    [SerializeField] private float _scaleSpeed = 2f;
    [SerializeField] private float _scaleMin = 0.9f;
    [SerializeField] private float _scaleMax = 1.1f;

    [Header("Debug")]
    [SerializeField, ReadOnly] private float _time;
    [SerializeField, ReadOnly] private float _delta;

    private void OnValidate()
    {
        if (_scaleObject == null) _scaleObject = transform;
    }

    private void Update()
    {
        _time += Time.unscaledDeltaTime * _scaleSpeed;
        _delta = Mathf.Sin(_time) * 0.5f + 0.5f;
        float s = Mathf.Lerp(_scaleMin, _scaleMax, _delta);
        _scaleObject.localScale = Vector3.one * s;
    }
}
