using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GunDataVisual : MonoBehaviour
{
    [SerializeField] private string _name = "Data Type";
    [SerializeField, Range(0, 1)] private float _value = 0.5f;

    [Header("References")]
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private Slider _valueSlider;

    private void OnValidate()
    {
        if (_nameText) _nameText.text = _name;
        if (_valueSlider) _valueSlider.value = _value;
    }
}
