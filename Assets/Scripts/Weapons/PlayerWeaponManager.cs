using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Sirenix.OdinInspector;

public class PlayerWeaponManager : MonoBehaviour
{
    [SerializeField] private LayerMask _bulletHitLayer = 1;

    [Header("Weapon Settings")]
    [SerializeField] private Transform _weaponHolder;
    [SerializeField] private float _pickupRadius = 1f;
    [SerializeField] private int _maxWeapons = 2; // Maximum number of weapons the player can hold
    [SerializeField, ReadOnly] private List<WeaponBase> _weapons = new List<WeaponBase>(); // List of held weapons
    public List<WeaponBase> Weapons => _weapons;
    public WeaponBase StarterWeapon => _weapons.Count > 0 ? _weapons[0] : null;

    [Header("UI References")]
    private TextMeshPro _pickupText;
    //private PlayerUI _playerUI;
    private SpriteRenderer _reloadBar;
    private SpriteRenderer _reloadBarBG;

    public WeaponBase EquippedWeapon => _equippedWeaponIndex < _weapons.Count ? _weapons[_equippedWeaponIndex] : null;
    private Camera _mainCamera;
    private Coroutine _reloadCoroutine;

    [SerializeField, ReadOnly] private int _equippedWeaponIndex = 0; // Index of the currently equipped weapon
    public int EquippedWeaponIndex => _equippedWeaponIndex;
    private bool _isInputEnabled = true;

    [Header("Ammo")]
    [SerializeField] private InventoryData _playerInventory;
    [SerializeField, ReadOnly] private int _equippedAmmoIndex = 0;
    public int EquippedAmmoIndex => _equippedAmmoIndex;

    public List<MineralData> Ammos => _playerInventory.EquippedAmmos;
    public MineralData EquippedAmmo => Ammos[_equippedAmmoIndex];
    public float CurrentAmmoInventory => _playerInventory.GetInventoryCount(EquippedAmmo);

    public static event System.Action<WeaponData> OnEquipWeapon = delegate { };
    public static event System.Action<MineralData> OnEquipAmmo = delegate { };

    private void Awake()
    {
        InitializeReferences();
        InitializeUI();
        UpdatePickupText();

        _equippedWeaponIndex = 0;
        for (int i = 0; i < _playerInventory.StartWeapons.Count; i++)
        {
            var weapon = Instantiate(_playerInventory.StartWeapons[i].WeaponPrefab, transform, false);
            _weapons.Add(weapon);
            _weapons[i].gameObject.SetActive(_equippedWeaponIndex == i);
            _weapons[i].Pickup(_weaponHolder);
        }
    }

    private void Start()
    {
        OnEquipAmmo?.Invoke(EquippedAmmo);
        OnEquipWeapon?.Invoke(EquippedWeapon != null ? EquippedWeapon.Data : null);
    }

    private void OnEnable()
    {
        // PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        // if (playerHealth != null)
        // {
        //     playerHealth.PlayerDeathEvent.AddListener(DisableInput);
        // }
    }

    private void OnDisable()
    {
        // PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        // if (playerHealth != null)
        // {
        //     playerHealth.PlayerDeathEvent.RemoveListener(DisableInput);
        // }
    }

    private void Update()
    {
        if (Time.timeScale == 0) return;
        if (!_isInputEnabled) return;

        UpdatePickupText();
        HandleWeaponPickupAndDrop();
        HandleWeaponSwitching();
        HandleAmmoSwitching();      // cycle ammo types
        HandleReloading();
        HandleAimingAndShooting();
        //UpdateAmmoUI();
    }

    public void DisableInput()
    {
        _isInputEnabled = false;
        HideReloadBar();
        _pickupText.gameObject.SetActive(false);
        EquippedWeapon.gameObject.SetActive(false);
    }

    private void InitializeReferences()
    {
        _mainCamera = Camera.main;

        Transform pickupTextTransform = transform.Find("PickupText");
        if (pickupTextTransform != null)
        {
            _pickupText = pickupTextTransform.GetComponent<TextMeshPro>();
        }
    }

    private void InitializeUI()
    {
        // TODO: Serialize
        _reloadBar = transform.Find("ReloadBar")?.GetComponent<SpriteRenderer>();
        _reloadBarBG = transform.Find("ReloadBarBG")?.GetComponent<SpriteRenderer>();

        HideReloadBar();
    }

    private void UpdatePickupText()
    {
        WeaponBase weaponInRange = GetWeaponInRange();
        if (_pickupText != null)
        {
            _pickupText.text = weaponInRange != null ? $"[E] Pick up {weaponInRange.WeaponName}" : "";
        }
    }

    private WeaponBase GetWeaponInRange()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, _pickupRadius);
        foreach (Collider col in colliders)
        {
            WeaponBase weapon = col.GetComponent<WeaponBase>();
            if (weapon != null && !weapon.IsEquipped)
            {
                return weapon;
            }
        }
        return null;
    }

    private void HandleWeaponPickupAndDrop()
    {
        if (PlayerInput.InteractOrSubmit)
        {
            if (IsOverPickup())
            {
                if (_weapons.Count >= _maxWeapons - 1) DropCurrentWeapon();
                PickupWeapon();
                HideReloadBar();
            }
        }
    }

    private void PickupWeapon()
    {
        WeaponBase weaponToPickup = GetWeaponInRange();
        if (weaponToPickup != null)
        {
            StopReloadCoroutines();

            if (_weapons.Count >= _maxWeapons)
            {
                // Drop the currently equipped weapon if the maximum number of weapons is reached
                DropCurrentWeapon();
            }

            weaponToPickup.Pickup(_weaponHolder);
            _weapons.Add(weaponToPickup); // Add the new weapon to the list
            _equippedWeaponIndex = _weapons.Count - 1; // Switch to the newly picked-up weapon
            UpdateCurrentWeapon();
        }
    }

    private void DropCurrentWeapon()
    {
        if (EquippedWeapon != null)
        {
            EquippedWeapon.Drop();
            _weapons.RemoveAt(_equippedWeaponIndex); // Remove the dropped weapon from the list

            // Adjust the current weapon index
            if (_weapons.Count > 0)
            {
                _equippedWeaponIndex %= _weapons.Count;
            }
            else
            {
                _equippedWeaponIndex = 0;
            }

            UpdateCurrentWeapon();
            //UpdateAmmoUI();
        }
    }

    private void StopReloadCoroutines()
    {
        if (_reloadCoroutine != null)
        {
            StopCoroutine(_reloadCoroutine);
            _reloadCoroutine = null;
        }

        if (EquippedWeapon != null)
        {
            EquippedWeapon.StopReload();
        }

        HideReloadBar();
    }

    private bool IsOverPickup()
    {
        // TODO: Add layer to only check for Weapon objects (This is expensive atm)
        Collider[] colliders = Physics.OverlapSphere(transform.position, _pickupRadius);
        foreach (Collider col in colliders)
        {
            if (col.GetComponent<WeaponBase>() != null)
            {
                return true;
            }
        }
        return false;
    }

    private void HandleAimingAndShooting()
    {
        if (EquippedWeapon == null) return;

        // TODO: Replace below with PlayerInput.MousePositionWorld?
        Vector3 mousePosition = _mainCamera.ScreenToWorldPoint(PlayerInput.MousePosition);
        mousePosition.z = 0f;

        EquippedWeapon.Aim(mousePosition);

        if (CanShoot())
        {
            StopReloadCoroutines();

            // Replace the old aiming code with a raycast (For perspective camera)
            var pos = PlayerInput.MousePositionWorld(transform.position.z);
            Vector3 aimDirection = (pos - transform.position).normalized;

            // tell the weapon to shoot select ammo type
            EquippedWeapon.Shoot(aimDirection, _bulletHitLayer, EquippedAmmo);
            //UpdateAmmoUI();
        }
    }

    private bool CanShoot()
    {
        if (EquippedWeapon == null || !EquippedWeapon.CanShoot() || ShowPopupOnHover.IsUIHovered) return false;

        return EquippedWeapon.IsAutomatic ? PlayerInput.MouseLeftHeld : PlayerInput.MouseLeftPressed;
    }

    private void HandleReloading()
    {
        if (EquippedWeapon == null || _reloadCoroutine != null) return;

        // Allow reload if R is pressed or if left click is pressed with no ammo in the mag
        if (PlayerInput.ReloadAmmo || (PlayerInput.MouseLeftPressed && EquippedWeapon.CurrentMagAmmo <= 0))
        {
            float ammoNeeded = EquippedWeapon.AmmoNeededForReload;
            float ammoAvailable = CurrentAmmoInventory;
            float ammoToReload = Mathf.Min(ammoNeeded, ammoAvailable);

            if (ammoToReload > 0)
            {
                EquippedWeapon.Reload(); // Ensure the weapon's reload logic is called
            }
        }
    }
        
    public void StartReloadCoroutine()
    {
        float ammoNeeded = EquippedWeapon.AmmoNeededForReload;
        float ammoAvailable = CurrentAmmoInventory;
        float ammoToReload = Mathf.Min(ammoNeeded, ammoAvailable);

        _reloadCoroutine = StartCoroutine(ReloadWithBar(EquippedWeapon.ReloadTime, ammoToReload));
    }

    private IEnumerator ReloadWithBar(float reloadTime, float ammoToReload)
    {
        ShowReloadBar();

        float elapsedTime = 0f;
        while (elapsedTime < reloadTime)
        {
            elapsedTime += Time.deltaTime;
            UpdateReloadBar(elapsedTime / reloadTime);

            if (EquippedWeapon == null)
            {
                // Weapon was dropped or swapped during reload
                //EquippedWeapon.StopCoroutine(EquippedWeapon.ReloadCoroutine());
                yield break;
            }

            yield return null;
        }
        CompleteReload(ammoToReload);
    }

    private void HideReloadBar()
    {
        if (_reloadBar != null) _reloadBar.enabled = false;
        if (_reloadBarBG != null) _reloadBarBG.enabled = false;
    }
    
    private void ShowReloadBar()
    {
        if (_reloadBar != null)
        {
            _reloadBar.transform.localScale = new Vector3(0, _reloadBar.transform.localScale.y, _reloadBar.transform.localScale.z);
            _reloadBar.enabled = true;
        }
        if (_reloadBarBG != null) _reloadBarBG.enabled = true;
    }

    private void UpdateReloadBar(float progress)
    {
        if (_reloadBar != null)
        {
            //  *2 is for the x value of bar's initial scale in the editor
            _reloadBar.transform.localScale = new Vector3(progress * 2, _reloadBar.transform.localScale.y, _reloadBar.transform.localScale.z);
        }
    }

    private void CompleteReload(float ammoToReload)
    {
        HideReloadBar();

        if (EquippedWeapon != null)
        {
            // Reload secondary weapon and update reserve ammo
            //_currentAmmoInventory[(int)_currentAmmoType] -= ammoToReload;
            //UpdateAmmoUI();
        }
    }

    private void HandleWeaponSwitching()
    {
        if (PlayerInput.MouseScroll > 0f) // Scroll up
        {
            SwitchToNextWeapon();
        }
        else if (PlayerInput.MouseScroll < 0f) // Scroll down
        {
            SwitchToPreviousWeapon();
        }
        else
        {
            for (int i = 0; i < _weapons.Count; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    _equippedWeaponIndex = i;
                    UpdateCurrentWeapon();
                    //UpdateAmmoUI();
                    break;
                }
            }
        }
    }

    private void SwitchToNextWeapon()
    {
        if (_weapons.Count == 0) return;

        _equippedWeaponIndex = (_equippedWeaponIndex + 1) % _weapons.Count;
        UpdateCurrentWeapon();
    }

    private void SwitchToPreviousWeapon()
    {
        if (_weapons.Count == 0) return;

        _equippedWeaponIndex = (_equippedWeaponIndex - 1 + _weapons.Count) % _weapons.Count;
        UpdateCurrentWeapon();
    }

    public void SwitchToWeapon(WeaponData weapon)
    {
        if (_weapons.Count == 0) return;
        for (int i = 0; i < _weapons.Count; i++)
        {
            if (_weapons[i].Data == weapon)
            {
                _equippedWeaponIndex = i;
                UpdateCurrentWeapon();
                return;
            }
        }
    }

    private void UpdateCurrentWeapon()
    {
        StopReloadCoroutines();
        UpdateWeaponVisibility();
        OnEquipWeapon?.Invoke(EquippedWeapon != null ? EquippedWeapon.Data : null);
        //WeaponChangeEvent.Invoke(_equippedWeapon);
        //UpdateAmmoUI();
    }

    private void UpdateWeaponVisibility()
    {
        for (int i = 0; i < _weapons.Count; i++)
        {
            _weapons[i].gameObject.SetActive(i == _equippedWeaponIndex);
        }
    }

    // Ammo Cycling
    private void HandleAmmoSwitching()
    {
        int count = Ammos.Count;

        if (PlayerInput.SwitchToPreviousAmmo || PlayerInput.SwitchToNextAmmo)
        {
            int direction = PlayerInput.SwitchToNextAmmo ? 1 : -1;
            _equippedAmmoIndex = (_equippedAmmoIndex + direction + count) % count;
            OnEquipAmmo?.Invoke(EquippedAmmo);
            //AmmoTypeChangeEvent.Invoke(EquippedAmmo);
            ResetAndReloadWeapon();
        }
    }

    public void SwitchToAmmo(MineralData ammo)
    {
        for (int i = 0; i < Ammos.Count; i++)
        {
            if (Ammos[i] == ammo)
            {
                _equippedAmmoIndex = i;
                OnEquipAmmo?.Invoke(EquippedAmmo);
                //AmmoTypeChangeEvent.Invoke(EquippedAmmo);
                ResetAndReloadWeapon();
                return;
            }
        }
    }

    private void ResetAndReloadWeapon()
    {
        if (EquippedWeapon != null)
        {
            EquippedWeapon.CurrentMagAmmo = 0; // Reset mag ammo when switching ammo types
            StopReloadCoroutines();
            EquippedWeapon.StopReload(); // Stop reloading if in progress
            EquippedWeapon.Reload(); // Start reloading if needed
        }
    }

    public void AddAmmo(int amount)
    {
        _playerInventory.AddItemToInventory(EquippedAmmo, amount);

        //UpdateAmmoUI();
    }

    public void SubtractAmmo(float amount)
    {
        _playerInventory.RemoveItemFromInventory(EquippedAmmo, amount);

        //UpdateAmmoUI();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _pickupRadius);
    }
}
