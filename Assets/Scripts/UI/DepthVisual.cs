using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DepthVisual : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    [SerializeField] private float _maxDepth = 1000f;

    private Transform _player;

    private void OnValidate()
    {
        if (_slider == null) _slider = GetComponent<Slider>();
    }

    private void Start()
    {
        _player = Player.Instance.transform;
    }

    private void Update()
    {
        float depth = Mathf.Clamp(-_player.position.y, 0f, _maxDepth);
        _slider.value = depth / _maxDepth;
    }
}
