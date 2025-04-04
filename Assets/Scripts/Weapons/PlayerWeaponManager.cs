using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class PlayerWeaponManager : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private Transform _weaponHolder;
    [SerializeField] private float _pickupRadius = 1f;
    [SerializeField] private WeaponBase _primaryWeapon; // Assign this in the Unity Inspector
    public WeaponBase StarterWeapon => _primaryWeapon;

    private WeaponBase _secondaryWeapon; // Secondary weapon (picked up weapon)

    [Header("UI References")]
    private TextMeshPro _pickupText;
    //private PlayerUI _playerUI;
    private SpriteRenderer _reloadBar;
    private SpriteRenderer _reloadBarBG;

    [Header("Events")]
    public UnityEvent<int> AmmoChangeEvent = new UnityEvent<int>();
    public UnityEvent<WeaponBase> WeaponChangeEvent = new UnityEvent<WeaponBase>();

    private WeaponBase _currentWeapon;
    public WeaponBase CurrentWeapon => _currentWeapon;
    private Camera _mainCamera;
    private Coroutine _reloadCoroutine;
    private bool _canDropWeapon = true;

    private float _globalAmmoPercentage = 100f;

    public float GlobalAmmoPercentage => _globalAmmoPercentage;

    public int MaxMagAmmo => _currentWeapon != null ? _currentWeapon.MaxMagSize : 0;
    public int CurrentAmmo => _currentWeapon != null ? _currentWeapon.CurrentAmmo : 0;

    private int _currentWeaponIndex = 0; // 0 for starter weapon, 1 for secondary weapon

    private bool _isInputEnabled = true;

    private void Awake()
    {
        InitializeReferences();
        InitializeUI();
        UpdatePickupText();
    }

    void Start()
    {
        if (_primaryWeapon != null)
        {
            _primaryWeapon.gameObject.SetActive(true); // Ensure the primary weapon is active
            _currentWeapon = _primaryWeapon; // Set the current weapon to the primary weapon
            _currentWeaponIndex = 0; // Set the current weapon index to 0 (primary weapon)
            _primaryWeapon.Pickup(_weaponHolder); // Parent the primary weapon to the weapon holder
            UpdateCurrentWeapon(); // Update the current weapon
        }
        else
        {
            Debug.LogWarning("Primary weapon not assigned in PlayerWeaponManager. Please assign a primary weapon in the inspector.");
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

        // _playerUI = FindObjectOfType<PlayerUI>();
        // if (_playerUI == null)
        // {
        //     Debug.LogWarning("PlayerUI component not found in the scene. Ammo bar will not be updated.");
        // }
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

            // If a secondary weapon already exists, drop it
            if (_secondaryWeapon != null)
            {
                _secondaryWeapon.Drop();
            }

            // Assign the new weapon to the secondary slot
            weaponToPickup.Pickup(_weaponHolder);
            int newAmmo = Mathf.FloorToInt(_globalAmmoPercentage / 100f * weaponToPickup.MaxAmmo);
            weaponToPickup.SetCurrentAmmo(newAmmo);

            _secondaryWeapon = weaponToPickup;

            // If the starter weapon is currently equipped, keep it equipped
            if (_currentWeapon == _primaryWeapon)
            {
                _currentWeaponIndex = 1; // Keep the starter weapon equipped
            }
            else
            {
                _currentWeaponIndex = 1; // Switch to the new secondary weapon
            }

            UpdateCurrentWeapon();
        }
    }

    private void DropCurrentWeapon()
    {
        if(_currentWeaponIndex == 0)
        {
            // Drop the primary weapon
            _primaryWeapon.Drop();
            _primaryWeapon = null; // Clear the reference to the dropped weapon
        }
        else
        {
            // Drop the secondary weapon
            _secondaryWeapon.Drop();
            _secondaryWeapon = null; // Clear the reference to the dropped weapon
        }
        UpdateCurrentWeapon();
        UpdateAmmoUI();
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

            Vector3 aimDirection = (mousePosition - _currentWeapon.transform.position).normalized;
            _currentWeapon.Shoot(aimDirection);
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
    if (_currentWeapon == null || _currentWeapon.IsReloading || _reloadCoroutine != null) return;

    // Allow reload if R is pressed or if left click is pressed with no ammo in the mag
    if (Input.GetKeyDown(KeyCode.R) || (Input.GetMouseButtonDown(0) && _currentWeapon.CurrentMagAmmo <= 0))
    {
        if (_currentWeapon.CurrentMagAmmo < _currentWeapon.MaxMagSize)
        {
            int ammoNeeded = _currentWeapon.MaxMagSize - _currentWeapon.CurrentMagAmmo;
            int ammoAvailable = Mathf.CeilToInt(_globalAmmoPercentage / 100f) * _currentWeapon.MaxAmmo; // Adjust for 0-100 range
            int ammoToReload = _currentWeapon == _primaryWeapon
                ? ammoNeeded // Starter weapon has infinite ammo
                : Mathf.Min(ammoNeeded, ammoAvailable);

            if (ammoToReload > 0)
            {
                _reloadCoroutine = StartCoroutine(ReloadWithBar(_currentWeapon.ReloadTime, ammoToReload));
                _currentWeapon.Reload(); // Ensure the weapon's reload logic is called
            }
        }
    }
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
            _reloadBar.transform.localScale = new Vector3(progress, _reloadBar.transform.localScale.y, _reloadBar.transform.localScale.z);
        }
    }

private void CompleteReload(int ammoToReload)
{
    HideReloadBar();

    if (_currentWeapon != null)
    {

        // Reload secondary weapon and update global ammo
        float ammoUsedPercentage = (float)ammoToReload / _currentWeapon.MaxAmmo * 100f; // Adjust for 0-100 range
        _globalAmmoPercentage -= ammoUsedPercentage;
        _globalAmmoPercentage = Mathf.Clamp(_globalAmmoPercentage, 0f, 100f);

        UpdateAmmoUI();
    }
}

private void UpdateAmmoUI()
{
    if (_currentWeapon != null)
    {
        AmmoChangeEvent.Invoke(_currentWeapon.CurrentAmmo);
        //_playerUI?.UpdateAmmoBar(_currentWeapon.CurrentMagAmmo);
    }
    else
    {
        AmmoChangeEvent.Invoke(0);
    }
}

public void AddAmmo(int percentAmount)
{
    _globalAmmoPercentage = Mathf.Clamp(_globalAmmoPercentage + percentAmount, 0f, 100f); // Adjust for 0-100 range

    if (_currentWeapon != null)
    {
        int newAmmo = Mathf.FloorToInt((_globalAmmoPercentage / 100f) * _currentWeapon.MaxAmmo); // Adjust for 0-100 range
        _currentWeapon.SetCurrentAmmo(newAmmo);
        UpdateAmmoUI();
    }
}

    public void DisableWeaponDropTemporarily()
    {
        _canDropWeapon = false;
        StartCoroutine(ReenableWeaponDrop());
    }

    private IEnumerator ReenableWeaponDrop()
    {
        yield return new WaitForSeconds(1f);
        _canDropWeapon = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _pickupRadius);
    }

    // Add a new method to handle weapon switching with the mouse wheel
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
        else if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _currentWeapon = _primaryWeapon;
            _currentWeaponIndex = 0;
            UpdateCurrentWeapon();
            UpdateAmmoUI();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && _secondaryWeapon != null)
        {
            _currentWeapon = _secondaryWeapon;
            _currentWeaponIndex = 1;
            UpdateCurrentWeapon();
            UpdateAmmoUI();
        }
    }

    private void SwitchToNextWeapon()
    {
        if (_secondaryWeapon == null) return; // No secondary weapon to switch to

        _currentWeaponIndex = (_currentWeaponIndex + 1) % 2; // Toggle between 0 and 1
        UpdateCurrentWeapon();
    }

    private void SwitchToPreviousWeapon()
    {
        if (_secondaryWeapon == null) return; // No secondary weapon to switch to

        _currentWeaponIndex = (_currentWeaponIndex - 1 + 2) % 2; // Toggle between 0 and 1
        UpdateCurrentWeapon();
    }

    private void UpdateCurrentWeapon()
    {
        StopReloadCoroutines();
        
        if (_currentWeaponIndex == 0)
        {
            _currentWeapon = _primaryWeapon;
        }
        else if (_currentWeaponIndex == 1)
        {
            _currentWeapon = _secondaryWeapon;
        }

        // Update weapon visibility
        UpdateWeaponVisibility();

        // Trigger events for UI updates
        WeaponChangeEvent.Invoke(_currentWeapon);
        UpdateAmmoUI();
        StopReloadCoroutines();
    }

    private void UpdateWeaponVisibility()
    {
        if (_primaryWeapon != null)
        {
            _primaryWeapon.gameObject.SetActive(_currentWeaponIndex == 0);
        }

        if (_secondaryWeapon != null)
        {
            _secondaryWeapon.gameObject.SetActive(_currentWeaponIndex == 1);
        }
    }
}