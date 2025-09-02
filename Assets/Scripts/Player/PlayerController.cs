using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : InitializedBehaviour
{

    [Header("Movement Settings")]
    private float _moveSpeed = 5f;
    private bool _isMove = false;

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
    [SerializeField] private Weapon _currentWeapon;
    private bool _wasShootingLastFrame;

    public override void Entry(params object[] dependencies)
    {
        _playerUIController = GetDependency<PlayerUIController>(dependencies);

        playerCamera = _cameraController.gameObject.GetComponent<Camera>();
        _rigidbody = GetComponent<Rigidbody>();

        weaponHolder.Initialize(playerCamera, _playerUIController);

        _rigidbody.freezeRotation = true;
        _wasShootingLastFrame = false;

        SetupInputProvider();
        EquipWeapon();
    }

    private void SetupInputProvider()
    {
        //_inputProvider = new JoystickInput(playerJoystick);
        _inputProvider = new KeyBoardInput();
        _cameraController.Init(_inputProvider, transform);
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
        if (_inputProvider == null || _currentWeapon == null) return;

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

        // Обработка цифровых клавиш для смены оружия
        if (Input.GetKeyDown(KeyCode.Alpha1)) EquipWeapon(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) EquipWeapon(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) EquipWeapon(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) EquipWeapon(3);

        // Колесико мыши для пролистывания оружия
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
        if (_inputProvider == null) return;

        Vector2 input = _inputProvider.GetMovementInput();

        if (input.magnitude > 0.1f)
        {
            // Двигаемся вперед-назад относительно взгляда игрока
            Vector3 moveDirection = new Vector3(input.x, 0, input.y);
            moveDirection = transform.TransformDirection(moveDirection);

            Vector3 movement = moveDirection * _moveSpeed * Time.fixedDeltaTime;
            _rigidbody.MovePosition(_rigidbody.position + movement);
            _isMove = true;
        }
        else
        {
            _isMove = false;
        }
    }

    #endregion

    public void SetMoveSpeed(float speed) => _moveSpeed = speed;
}
