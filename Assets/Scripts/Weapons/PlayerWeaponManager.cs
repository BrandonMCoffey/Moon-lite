using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerWeaponManager : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private Transform _weaponHolder;
    [SerializeField] private float _pickupRadius = 1f;
    [SerializeField] private int _maxWeapons = 2; // Maximum number of weapons the player can hold
    private List<WeaponBase> _weapons = new List<WeaponBase>(); // List of held weapons
    public List<WeaponBase> Weapons => _weapons;
    public WeaponBase StarterWeapon => _weapons.Count > 0 ? _weapons[0] : null;

    [Header("UI References")]
    private TextMeshPro _pickupText;
    //private PlayerUI _playerUI;
    private SpriteRenderer _reloadBar;
    private SpriteRenderer _reloadBarBG;

    private WeaponBase _equippedWeapon;
    public WeaponBase EquippedWeapon => _equippedWeapon;
    private Camera _mainCamera;
    private Coroutine _reloadCoroutine;
    private bool _canDropWeapon = true;

    private int _equippedWeaponIndex = 0; // Index of the currently equipped weapon
    public int EquippedWeaponIndex => _equippedWeaponIndex;
    private bool _isInputEnabled = true;

    [Header("Ammo")]
    [SerializeField] private InventoryData _playerInventory;
    [SerializeField] private int _equippedAmmoIndex = 0;
    public int EquippedAmmoIndex => _equippedAmmoIndex;

    [SerializeField] private List<MineralData> _ammoDataEquipped;
    public List<MineralData> Ammos => _ammoDataEquipped;
    public MineralData EquippedAmmo => _ammoDataEquipped[_equippedAmmoIndex];
    public int CurrentAmmoInventory => _playerInventory.GetInventoryCount(EquippedAmmo);

    public static event System.Action<WeaponData> OnEquipWeapon = delegate { };
    public static event System.Action<MineralData> OnEquipAmmo = delegate { };

    private void Awake()
    {
        InitializeReferences();
        InitializeUI();
        UpdatePickupText();

        if(GetComponentInChildren<WeaponBase>() != null)
        {
            foreach (var weapon in GetComponentsInChildren<WeaponBase>())
            {
                if (weapon != null && weapon.gameObject != gameObject) // Avoid adding the manager itself
                {
                    _weapons.Add(weapon);
                }
            }
        }
    }

    private void Start()
    {
        if (_weapons.Count > 0)
        {
            foreach (var weapon in _weapons)
            {
                weapon.gameObject.SetActive(false); // Ensure all weapons are initially inactive
                weapon.Pickup(_weaponHolder); // Parent the weapon to the weapon holder
            }

            _equippedWeapon = _weapons[0]; // Set the current weapon to the first weapon in the list
            _equippedWeaponIndex = 0;
            _equippedWeapon.gameObject.SetActive(true); // Activate the current weapon
            UpdateCurrentWeapon(); // Update the current weapon

            // Make sure references have equipped ammo
            OnEquipAmmo?.Invoke(EquippedAmmo);
        }
        else
        {
            Debug.LogWarning("No weapons assigned in PlayerWeaponManager. Please assign at least one weapon in the inspector.");
        }
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
        _equippedWeapon.gameObject.SetActive(false);
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
        _reloadBar = GameObject.Find("ReloadBar")?.GetComponent<SpriteRenderer>();
        _reloadBarBG = GameObject.Find("ReloadBarBG")?.GetComponent<SpriteRenderer>();

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
        if (PlayerInput.PickupDropWeapon)
        {
            if (IsOverPickup())
            {
                PickupWeapon();
                HideReloadBar();
            }
            else if (_equippedWeapon != null && _canDropWeapon)
            {
                DropCurrentWeapon();
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
        if (_equippedWeapon != null)
        {
            _equippedWeapon.Drop();
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

        if (_equippedWeapon != null)
        {
            _equippedWeapon.StopReload();
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
        if (_equippedWeapon == null) return;

        // TODO: Replace below with PlayerInput.MousePositionWorld?
        Vector3 mousePosition = _mainCamera.ScreenToWorldPoint(PlayerInput.MousePosition);
        mousePosition.z = 0f;

        _equippedWeapon.Aim(mousePosition);

        if (CanShoot())
        {
            StopReloadCoroutines();

            // Replace the old aiming code with a raycast (For perspective camera)
            var pos = PlayerInput.MousePositionWorld(transform.position.z);
            Vector3 aimDirection = (pos - transform.position).normalized;

            // tell the weapon to shoot select ammo type
            _equippedWeapon.Shoot(aimDirection, EquippedAmmo);
            //UpdateAmmoUI();
        }
    }

    private bool CanShoot()
    {
        if (_equippedWeapon == null || !EquippedWeapon.CanShoot()) return false;

        return _equippedWeapon.IsAutomatic ? PlayerInput.MouseLeftHeld : PlayerInput.MouseLeftPressed;
    }

    private void HandleReloading()
    {
        if (_equippedWeapon == null || _reloadCoroutine != null) return;

        // Allow reload if R is pressed or if left click is pressed with no ammo in the mag
        if (PlayerInput.ReloadAmmo || (PlayerInput.MouseLeftPressed && _equippedWeapon.CurrentMagAmmo <= 0))
        {
            int ammoNeeded = _equippedWeapon.AmmoNeededForReload;
            int ammoAvailable = CurrentAmmoInventory;
            int ammoToReload = Mathf.Min(ammoNeeded, ammoAvailable);

            if (ammoToReload > 0)
            {
                _equippedWeapon.Reload(); // Ensure the weapon's reload logic is called
            }
        }
    }
        
    public void StartReloadCoroutine()
    {
        int ammoNeeded = _equippedWeapon.AmmoNeededForReload;
        int ammoAvailable = CurrentAmmoInventory;
        int ammoToReload = Mathf.Min(ammoNeeded, ammoAvailable);

        _reloadCoroutine = StartCoroutine(ReloadWithBar(_equippedWeapon.ReloadTime, ammoToReload));
    }

    private IEnumerator ReloadWithBar(float reloadTime, int ammoToReload)
    {
        ShowReloadBar();

        float elapsedTime = 0f;
        while (elapsedTime < reloadTime)
        {
            elapsedTime += Time.deltaTime;
            UpdateReloadBar(elapsedTime / reloadTime);

            if (_equippedWeapon == null)
            {
                // Weapon was dropped or swapped during reload
                _equippedWeapon.StopCoroutine(_equippedWeapon.ReloadCoroutine());
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

    private void CompleteReload(int ammoToReload)
    {
        HideReloadBar();

        if (_equippedWeapon != null)
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

    private void UpdateCurrentWeapon()
    {
        StopReloadCoroutines();

        if (_weapons.Count > 0)
        {
            _equippedWeapon = _weapons[_equippedWeaponIndex];
        }
        else
        {
            _equippedWeapon = null;
        }

        UpdateWeaponVisibility();
        if (EquippedWeapon != null) OnEquipWeapon?.Invoke(EquippedWeapon.Data);
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
        int count = _ammoDataEquipped.Count;

        if (PlayerInput.SwitchToPreviousAmmo || PlayerInput.SwitchToNextAmmo)
        {
            int direction = PlayerInput.SwitchToNextAmmo ? 1 : -1;
            _equippedAmmoIndex = (_equippedAmmoIndex + direction + count) % count;
            OnEquipAmmo?.Invoke(EquippedAmmo);
            //AmmoTypeChangeEvent.Invoke(EquippedAmmo);
            ResetAndReloadWeapon();
        }
    }

    private void ResetAndReloadWeapon()
    {
        if (_equippedWeapon != null)
        {
            _equippedWeapon.CurrentMagAmmo = 0; // Reset mag ammo when switching ammo types
            StopReloadCoroutines();
            _equippedWeapon.StopReload(); // Stop reloading if in progress
            _equippedWeapon.Reload(); // Start reloading if needed
        }
    }

    public void AddAmmo(int amount)
    {
        _playerInventory.AddItemToInventory(EquippedAmmo, amount);

        //UpdateAmmoUI();
    }

    public void SubtractAmmo(int amount)
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
