using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthVisual : MonoBehaviour
{
    [SerializeField] private Slider _slider;

    private void OnValidate()
    {
        if (_slider == null) _slider = GetComponent<Slider>();
    }

    private void OnEnable()
    {
        // TODO: Change this to actual player health
        Player.OnHealthChanged += UpdateSlider;
    }

    private void OnDisable()
    {
        Player.OnHealthChanged -= UpdateSlider;
    }

    private void UpdateSlider(float value)
    {
        _slider.value = value;
    }
}
