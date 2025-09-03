using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : InitializedBehaviour
{

    [Header("Settings")]
    [SerializeField] private PlayerSettings settings;

    [Header("Health Settings")]
    private float _currHealth;

    [Header("Movement Settings")]
    private bool _isMove = false;
    private bool _isDied;

    [Header("Components")]
    [SerializeField] private PlayerCamera _cameraController;
    [SerializeField] private WeaponHolder weaponHolder;
    [SerializeField] private Joystick playerJoystick;
    private Camera playerCamera;
    private Rigidbody _rigidbody;
    private IInputProvider _inputProvider;

    [Header("Dependencies")]
    private PlayerUIController _playerUIController;

    [Header("Weapon Settings")]
    private Weapon _currentWeapon;
    private bool _wasShootingLastFrame;

    public Action onPlayerDieded;

    public bool IsDied { get => _isDied; }

    public override void Entry(params object[] dependencies)
    {
        _playerUIController = GetDependency<PlayerUIController>(dependencies);

        playerCamera = _cameraController.gameObject.GetComponent<Camera>();
        _rigidbody = GetComponent<Rigidbody>();

        weaponHolder.Initialize(playerCamera, _playerUIController);

        _rigidbody.freezeRotation = true;
        _wasShootingLastFrame = false;

        _currHealth = settings.startHealth;
        _playerUIController.SetCurrHealth(_currHealth);
        _isDied = false;

        Subscribed();
        SetupInputProvider();
        EquipWeapon();
    }

    private void Subscribed()
    {
        EnemyBase.onPlayerDamaged += TakeDamage;
    }

    private void Unsubscribed()
    {
        EnemyBase.onPlayerDamaged -= TakeDamage;
    }

    private void SetupInputProvider()
    {
        _inputProvider = new KeyBoardInput();
        _cameraController.Init(_inputProvider, transform, settings);
    }

    void Update()
    {
        HandleWeaponInput();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    #region Player Wapon

    private void HandleWeaponInput()
    {
        if (_inputProvider == null || _currentWeapon == null || _isDied) return;

        // Обработка стрельбы для разных типов input providers
        if (_inputProvider is KeyBoardInput)
        {
            HandleKeyboardWeaponInput();
        }
        else
        {
            HandleTouchWeaponInput();
        }

        // Обработка перезарядки
        if (_inputProvider.GetReloadInput())
        {
            _currentWeapon.Reload();
        }
    }

    private void HandleKeyboardWeaponInput()
    {
        bool isShootingInput = _inputProvider.GetShootInput();
        HandleWeaponSwitchInput();

        switch (_currentWeapon.Settings.fireMode)
        {
            case FireMode.Single:
                if (isShootingInput && !_wasShootingLastFrame)
                {
                    _currentWeapon.StartShooting();
                }
                break;

            case FireMode.Auto:
                if (isShootingInput && !_wasShootingLastFrame)
                {
                    _currentWeapon.StartShooting();
                }
                else if (!isShootingInput && _wasShootingLastFrame)
                {
                    _currentWeapon.StopShooting();
                }
                break;

            case FireMode.Burst:
                if (isShootingInput && !_wasShootingLastFrame)
                {
                    _currentWeapon.StartShooting();
                }
                break;
        }

        _wasShootingLastFrame = isShootingInput;

        if (_inputProvider.GetReloadInput())
        {
            _currentWeapon.Reload();
        }
    }

    private void HandleWeaponSwitchInput()
    {
        if (weaponHolder == null) return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) EquipWeapon(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) EquipWeapon(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) EquipWeapon(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) EquipWeapon(3);

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            weaponHolder.EquipNextWeapon();
        }
        else if (scroll < 0f)
        {
            weaponHolder.EquipPreviousWeapon();
        }
        _currentWeapon = weaponHolder.CurrentWeapon;
    }

    private void HandleTouchWeaponInput()
    {
        if (_inputProvider.GetShootInput())
        {
            _currentWeapon.StartShooting();
        }
        else
        {
            _currentWeapon.StopShooting();
        }
    }

    public void EquipWeapon(int weaponIndex = 0)
    {
        weaponHolder.EquipWeapon(weaponIndex);
        _currentWeapon = weaponHolder.CurrentWeapon;
    }

    public Weapon GetCurrentWeapon() => weaponHolder.CurrentWeapon;

    #endregion

    #region Player Move

    private void HandleMovement()
    {
        if (_inputProvider == null || _isDied) return;

        Vector2 input = _inputProvider.GetMovementInput();

        if (input.magnitude > 0.1f)
        {
            Vector3 moveDirection = new Vector3(input.x, 0, input.y);
            moveDirection = transform.TransformDirection(moveDirection);

            Vector3 movement = moveDirection * settings.moveSpeed * Time.fixedDeltaTime;
            _rigidbody.MovePosition(_rigidbody.position + movement);
            _isMove = true;
        }
        else
        {
            _isMove = false;
        }
    }

    #endregion

    #region Take Damage

    public void TakeDamage(float damage)
    {
        if (_isDied) return;

        _currHealth -= damage;
        if(_currHealth <= 0f)
        {
            _currHealth = 0f;
            PlayerDie();
        }
        _playerUIController.SetCurrHealth(_currHealth);
    }

    private void PlayerDie()
    {
        _isDied = true;
        _cameraController.PlayerDied();
        onPlayerDieded?.Invoke();
    }

    #endregion

    private void OnDestroy()
    {
        Unsubscribed();
    }

    public void SetMoveSpeed(float speed) => settings.moveSpeed = speed;
}
