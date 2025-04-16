using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class PlayerWeaponManager : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private Transform _weaponHolder;
    [SerializeField] private float _pickupRadius = 1f;
    [SerializeField] private int _maxWeapons = 2; // Maximum number of weapons the player can hold
    [SerializeField] private List<WeaponBase> _weapons = new List<WeaponBase>(); // List of held weapons
    public WeaponBase StarterWeapon => _weapons.Count > 0 ? _weapons[0] : null;

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

    private float _gunAmmoPercentage = 100f;

    public float GunAmmoPercentage => _gunAmmoPercentage;

    public int MaxMagAmmo => _currentWeapon != null ? _currentWeapon.MaxMagSize : 0;
    public int CurrentAmmo => _currentWeapon != null ? _currentWeapon.CurrentAmmo : 0;

    private int _currentWeaponIndex = 0; // Index of the currently equipped weapon

    private bool _isInputEnabled = true;

    private void Awake()
    {
        InitializeReferences();
        InitializeUI();
        UpdatePickupText();
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

            if (_weapons.Count >= _maxWeapons)
            {
                // Drop the currently equipped weapon if the maximum number of weapons is reached
                DropCurrentWeapon();
            }

            weaponToPickup.Pickup(_weaponHolder);
            int newAmmo = Mathf.FloorToInt(_gunAmmoPercentage / 100f * weaponToPickup.MaxAmmo);
            weaponToPickup.SetCurrentAmmo(newAmmo);

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
                int ammoAvailable = Mathf.CeilToInt(_gunAmmoPercentage / 100f) * _currentWeapon.MaxAmmo; // Adjust for 0-100 range
                int ammoToReload = _currentWeapon == _weapons[0]
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
            //  *2 is for the x value of bar's initial scale in the editor
            _reloadBar.transform.localScale = new Vector3(progress * 2, _reloadBar.transform.localScale.y, _reloadBar.transform.localScale.z);
        }
    }

    private void CompleteReload(int ammoToReload)
    {
        HideReloadBar();

        if (_currentWeapon != null)
        {

            // Reload secondary weapon and update global ammo
            float ammoUsedPercentage = (float)ammoToReload / _currentWeapon.MaxAmmo * 100f; // Adjust for 0-100 range
            _gunAmmoPercentage -= ammoUsedPercentage;
            _gunAmmoPercentage = Mathf.Clamp(_gunAmmoPercentage, 0f, 100f);

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
        _gunAmmoPercentage = Mathf.Clamp(_gunAmmoPercentage + percentAmount, 0f, 100f); // Adjust for 0-100 range

        if (_currentWeapon != null)
        {
            int newAmmo = Mathf.FloorToInt((_gunAmmoPercentage / 100f) * _currentWeapon.MaxAmmo); // Adjust for 0-100 range
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
}