using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GunInfoSlider : MonoBehaviour
{
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private Slider _valueSlider;

    public void SetInfo(string name, float valuePercent)
    {
        if (_nameText) _nameText.text = name;
        if (_valueSlider) _valueSlider.value = valuePercent;
    }
}
