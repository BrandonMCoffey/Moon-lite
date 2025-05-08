using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class HealthVisual : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    [SerializeField] private bool _snapIncrements;
    [SerializeField, ShowIf(nameof(_snapIncrements))] private int _incrementCount = 10;

    private void OnValidate()
    {
        if (_slider == null) _slider = GetComponent<Slider>();
    }

    private void OnEnable()
    {
        Player.OnHealthChanged += UpdateSlider;
    }

    private void OnDisable()
    {
        Player.OnHealthChanged -= UpdateSlider;
    }

    private void UpdateSlider(float value)
    {
        if (_snapIncrements)
        {
            value = Mathf.Round(value * _incrementCount) / _incrementCount;
        }
        _slider.value = value;
    }
}
