using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GunInfo : MonoBehaviour
{
    [SerializeField] private WeaponData _weapon;

    [Header("References")]
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private Image _weaponIcon;
    [SerializeField] private TMP_Text _descriptionText;
    [SerializeField] private List<GunInfoSlider> _sliders;

    private void OnValidate()
    {
        if (_weapon == null) return;

        if (_nameText) _nameText.text = _weapon.Name;
        if (_weaponIcon) _weaponIcon.sprite = _weapon.Sprite;
        if (_descriptionText) _descriptionText.text = _weapon.Description;

        int i = 0;
        _sliders[i++].SetInfo("Damage", _weapon.BulletDamage / 4);
        _sliders[i++].SetInfo("Knockback", _weapon.BulletKnockback / 2);
        if (_weapon.MultipleBullets) _sliders[i++].SetInfo("Bullets", _weapon.BulletAmount / 4);
        if (_weapon.HitScan) _sliders[i++].SetInfo("Laser Range", _weapon.RayRange / 20);
        if (_weapon.ForceAoe) _sliders[i++].SetInfo("AOE", _weapon.BulletAoeRadius / 4);
        for (int j = 0; j < _sliders.Count; j++)
        {
            _sliders[j].gameObject.SetActive(j < i);
        }
    }
}
