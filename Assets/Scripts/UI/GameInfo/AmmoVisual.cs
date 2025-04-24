using UnityEngine;

public class AmmoVisual : ItemVisual
{
    [SerializeField] private GameObject _selectedVisual;

    protected override void OnEnable()
    {
        base.OnEnable();
        PlayerWeaponManager.OnEquipAmmo += EquipAmmo;
        // TODO: Get equipped ammo (Singleton?)
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        PlayerWeaponManager.OnEquipAmmo -= EquipAmmo;
    }

    private void EquipAmmo(MineralData equipped)
    {
        _selectedVisual.SetActive(_itemData == equipped);
    }
}
