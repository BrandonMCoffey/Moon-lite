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
