using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class JetpackVisual : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    [SerializeField] private bool _snapIncrements;
    [SerializeField, ShowIf(nameof(_snapIncrements))] private int _incrementCount = 10;

    private PlayerMovementCustom _player;

    private void OnValidate()
    {
        if (_slider == null) _slider = GetComponent<Slider>();
    }

    private void Start()
    {
        _player = Player.Movement;
    }

    private void Update()
    {
        UpdateSlider(_player.JetpackFuel);
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
