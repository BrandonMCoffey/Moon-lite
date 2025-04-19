using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

// AmmoType enum here so the manager, weapons, and bullets all reference the same list
public enum AmmoType
{
    Basic,
    Explosive,
    Piercing
}

public class PlayerWeaponManager : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private Transform _weaponHolder;
    [SerializeField] private float _pickupRadius = 1f;
    [SerializeField] private int _maxWeapons = 2; // Maximum number of weapons the player can hold
    private List<WeaponBase> _weapons = new List<WeaponBase>(); // List of held weapons
    public WeaponBase StarterWeapon => _weapons.Count > 0 ? _weapons[0] : null;

    [Header("UI References")]
    private TextMeshPro _pickupText;
    //private PlayerUI _playerUI;
    private SpriteRenderer _reloadBar;
    private SpriteRenderer _reloadBarBG;

    private WeaponBase _currentWeapon;
    public WeaponBase CurrentWeapon => _currentWeapon;
    private Camera _mainCamera;
    private Coroutine _reloadCoroutine;
    private bool _canDropWeapon = true;

    private int _currentWeaponIndex = 0; // Index of the currently equipped weapon
    private bool _isInputEnabled = true;

    [Header("Ammo Types")]
    [SerializeField] private AmmoType _currentAmmoType = AmmoType.Basic;
    public AmmoType CurrentAmmoType => _currentAmmoType;

    [Header("Ammo Prefabs per AmmoType")]
    [Tooltip("Bullet prefab for each AmmoType (order must match the AmmoType enum)")]
    [SerializeField] private GameObject[] _ammoPrefabs;

    [Header("Ammo Inventory per Type")]
    [Tooltip("Reserve ammo for each AmmoType (order matches AmmoType enum)")]
    [SerializeField] private int[] _currentAmmoReserve;
    [Tooltip("Max reserve ammo per AmmoType")]
    [SerializeField] private int[] _maxAmmoInventory;

    public int CurrentMagAmmo => _currentWeapon != null ? _currentWeapon.CurrentMagAmmo : 0;
    public int CurrentAmmoInventory => _currentAmmoReserve[(int)_currentAmmoType];

    [Header("Events")]
    public UnityEvent<int> MagAmmoChangeEvent = new UnityEvent<int>();
    public UnityEvent<WeaponBase> WeaponChangeEvent = new UnityEvent<WeaponBase>();
    public UnityEvent<AmmoType> AmmoTypeChangeEvent = new UnityEvent<AmmoType>();

    private void Awake()
    {
        InitializeReferences();
        InitializeUI();
        UpdatePickupText();

        // Ensure prefab & inventory arrays match the enum length
        int types = System.Enum.GetValues(typeof(AmmoType)).Length;
        if (_ammoPrefabs.Length != types || _currentAmmoReserve.Length != types || _maxAmmoInventory.Length != types)
        {
            Debug.LogWarning($"AmmoType arrays must be length {types}; resetting to defaults.");
            _ammoPrefabs = new GameObject[types];
            _currentAmmoReserve = new int[types];
            _maxAmmoInventory = new int[types];
            for (int i = 0; i < types; i++)
                _maxAmmoInventory[i] = 100;
        }

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

    void Start()
    {
        if (_weapons.Count > 0)
        {
            foreach (var weapon in _weapons)
            {
                weapon.gameObject.SetActive(false); // Ensure all weapons are initially inactive
                weapon.Pickup(_weaponHolder); // Parent the weapon to the weapon holder
            }

            _currentWeapon = _weapons[0]; // Set the current weapon to the first weapon in the list
            _currentWeaponIndex = 0;
            _currentWeapon.gameObject.SetActive(true); // Activate the current weapon
            UpdateCurrentWeapon(); // Update the current weapon
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
        UpdateAmmoUI();
    }

    public void DisableInput()
    {
        _isInputEnabled = false;
        HideReloadBar();
        _pickupText.gameObject.SetActive(false);
        _currentWeapon.gameObject.SetActive(false);
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
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (IsOverPickup())
            {
                PickupWeapon();
                HideReloadBar();
            }
            else if (_currentWeapon != null && _canDropWeapon)
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
            _currentWeaponIndex = _weapons.Count - 1; // Switch to the newly picked-up weapon
            UpdateCurrentWeapon();
        }
    }

    private void DropCurrentWeapon()
    {
        if (_currentWeapon != null)
        {
            _currentWeapon.Drop();
            _weapons.RemoveAt(_currentWeaponIndex); // Remove the dropped weapon from the list

            // Adjust the current weapon index
            if (_weapons.Count > 0)
            {
                _currentWeaponIndex %= _weapons.Count;
            }
            else
            {
                _currentWeaponIndex = 0;
            }

            UpdateCurrentWeapon();
            UpdateAmmoUI();
        }
    }

    private void StopReloadCoroutines()
    {
        if (_reloadCoroutine != null)
        {
            StopCoroutine(_reloadCoroutine);
            _reloadCoroutine = null;
        }

        if (_currentWeapon != null)
        {
            _currentWeapon.StopReload();
        }

        HideReloadBar();
    }

    private bool IsOverPickup()
    {
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
        if (_currentWeapon == null) return;

        Vector3 mousePosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;

        _currentWeapon.Aim(mousePosition);

        if (CanShoot())
        {
            StopReloadCoroutines();

            // Replace the old aiming code with a raycast (For perspective camera)
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane xy = new Plane(Vector3.forward, new Vector3(0, 0, transform.position.z));
            xy.Raycast(ray, out float distance);
            var pos = ray.GetPoint(distance);
            Vector3 aimDirection = (pos - transform.position).normalized;

            // select prefab from manager, then tell the weapon to shoot it
            GameObject prefab = _ammoPrefabs[(int)_currentAmmoType];
            _currentWeapon.Shoot(aimDirection, prefab);
            UpdateAmmoUI();
        }
    }

    private bool CanShoot()
    {
        if (_currentWeapon == null || _currentWeapon.CurrentMagAmmo <= 0) return false;

        if (_currentWeapon != null)
        {
            return _currentWeapon.IsAutomatic ? Input.GetMouseButton(0) : Input.GetMouseButtonDown(0);
        }
        return Input.GetMouseButtonDown(0);
    }

    private void HandleReloading()
    {
        if (_currentWeapon == null || _reloadCoroutine != null) return;

        // Allow reload if R is pressed or if left click is pressed with no ammo in the mag
        if (Input.GetKeyDown(KeyCode.R) || (Input.GetMouseButtonDown(0) && _currentWeapon.CurrentMagAmmo <= 0))
        {
            int ammoNeeded = _currentWeapon.AmmoNeededForReload;
            int ammoAvailable = _currentAmmoReserve[(int)_currentAmmoType];
            int ammoToReload = Mathf.Min(ammoNeeded, ammoAvailable);

            if (ammoToReload > 0)
            {
                _currentWeapon.Reload(); // Ensure the weapon's reload logic is called
            }
        }
    }
        
    public void StartReloadCoroutine()
    {
        int ammoNeeded = _currentWeapon.AmmoNeededForReload;
        int ammoAvailable = _currentAmmoReserve[(int)_currentAmmoType];
        int ammoToReload = Mathf.Min(ammoNeeded, ammoAvailable);

        _reloadCoroutine = StartCoroutine(ReloadWithBar(_currentWeapon.ReloadTime, ammoToReload));
    }

    private IEnumerator ReloadWithBar(float reloadTime, int ammoToReload)
    {
        ShowReloadBar();

        float elapsedTime = 0f;
        while (elapsedTime < reloadTime)
        {
            elapsedTime += Time.deltaTime;
            UpdateReloadBar(elapsedTime / reloadTime);

            if (_currentWeapon == null)
            {
                // Weapon was dropped or swapped during reload
                _currentWeapon.StopCoroutine(_currentWeapon.ReloadCoroutine());
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

        if (_currentWeapon != null)
        {
            // Reload secondary weapon and update reserve ammo
            //_currentAmmoInventory[(int)_currentAmmoType] -= ammoToReload;
            UpdateAmmoUI();
        }
    }

    private void UpdateAmmoUI()
    {
        MagAmmoChangeEvent.Invoke(CurrentMagAmmo);
    }

    private void HandleWeaponSwitching()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0f) // Scroll up
        {
            SwitchToNextWeapon();
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f) // Scroll down
        {
            SwitchToPreviousWeapon();
        }
        else
        {
            for (int i = 0; i < _weapons.Count; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    _currentWeaponIndex = i;
                    UpdateCurrentWeapon();
                    UpdateAmmoUI();
                    break;
                }
            }
        }
    }

    private void SwitchToNextWeapon()
    {
        if (_weapons.Count == 0) return;

        _currentWeaponIndex = (_currentWeaponIndex + 1) % _weapons.Count;
        UpdateCurrentWeapon();
    }

    private void SwitchToPreviousWeapon()
    {
        if (_weapons.Count == 0) return;

        _currentWeaponIndex = (_currentWeaponIndex - 1 + _weapons.Count) % _weapons.Count;
        UpdateCurrentWeapon();
    }

    private void UpdateCurrentWeapon()
    {
        StopReloadCoroutines();

        if (_weapons.Count > 0)
        {
            _currentWeapon = _weapons[_currentWeaponIndex];
        }
        else
        {
            _currentWeapon = null;
        }

        UpdateWeaponVisibility();
        WeaponChangeEvent.Invoke(_currentWeapon);
        UpdateAmmoUI();
    }

    private void UpdateWeaponVisibility()
    {
        for (int i = 0; i < _weapons.Count; i++)
        {
            _weapons[i].gameObject.SetActive(i == _currentWeaponIndex);
        }
    }

    // Ammo Cycling
    private void HandleAmmoSwitching()
    {
        int count = System.Enum.GetValues(typeof(AmmoType)).Length;

        if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.G))
        {
            int direction = Input.GetKeyDown(KeyCode.F) ? 1 : -1;
            _currentAmmoType = (AmmoType)(((int)_currentAmmoType + direction + count) % count);
            AmmoTypeChangeEvent.Invoke(_currentAmmoType);
            ResetAndReloadWeapon();
        }
    }

    private void ResetAndReloadWeapon()
    {
        if (_currentWeapon != null)
        {
            _currentWeapon.CurrentMagAmmo = 0; // Reset mag ammo when switching ammo types
            StopReloadCoroutines();
            _currentWeapon.StopReload(); // Stop reloading if in progress
            _currentWeapon.Reload(); // Start reloading if needed
        }
    }

    public void AddAmmo(int amount)
    {
        int idx = (int)_currentAmmoType;
        _currentAmmoReserve[idx] = Mathf.Clamp(_currentAmmoReserve[idx] + amount, 0, _maxAmmoInventory[idx]);

        UpdateAmmoUI();
    }

    public void SubtractAmmo(int amount)
    {
        int idx = (int)_currentAmmoType;
        _currentAmmoReserve[idx] = Mathf.Clamp(_currentAmmoReserve[idx] - amount, 0, _maxAmmoInventory[idx]);

        UpdateAmmoUI();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _pickupRadius);
    }
}
