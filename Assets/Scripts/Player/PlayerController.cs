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
    [SerializeField] private PlayerAnim playerAnim;
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
        DefineDirection();
    }

    #region Player Wapon

    private void HandleWeaponInput()
    {
        if (_inputProvider == null || _currentWeapon == null || _isDied) return;

        if (_inputProvider is KeyBoardInput)
        {
            HandleKeyboardWeaponInput();
        }

        if (_inputProvider.GetReloadInput())
        {
            _currentWeapon.Reload();
        }

        if (_currentWeapon != null)
        {
            ReloadWeaponAnim(_currentWeapon.IsReloading);
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
            int nextind = weaponHolder.EquipNextWeapon();
            EquipWeapon(nextind);
        }
        else if (scroll < 0f)
        {
            int prewInd = weaponHolder.EquipPreviousWeapon();
            EquipWeapon(prewInd);
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
        if (!weaponHolder.IsCurrWeapon(weaponIndex)) 
            playerAnim.SetReload(false);

        weaponHolder.EquipWeapon(weaponIndex);
        _currentWeapon = weaponHolder.CurrentWeapon;
    }

    private void ReloadWeaponAnim(bool reloadState)
    {
        playerAnim.SetReload(reloadState);
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

    private void DefineDirection()
    {
        if (_inputProvider == null) return;

        Vector2 input = _inputProvider.GetMovementInput();

        if (input.magnitude <= 0.1f)
        {
            playerAnim.SetActiveRun(false);
            playerAnim.SetActiveRight(false);
            playerAnim.SetActiveLeft(false);
            playerAnim.SetActiveBack(false);
            return;
        }

        Vector3 worldDirection = transform.TransformDirection(new Vector3(input.x, 0, input.y));
        worldDirection.Normalize();

        float angle = Vector3.SignedAngle(transform.forward, worldDirection, Vector3.up);
        bool isMovingForward = Mathf.Abs(angle) < 45f;
        bool isMovingBackward = Mathf.Abs(angle) > 135f;
        bool isMovingRight = angle > 0 && angle <= 135f;
        bool isMovingLeft = angle < 0 && angle >= -135f;

        playerAnim.SetActiveRun(_isMove);
        if (isMovingForward)
        {
            playerAnim.SetActiveBack(false);
            playerAnim.SetActiveRight(false);
            playerAnim.SetActiveLeft(false);
        }
        else if (isMovingBackward)
        {
            playerAnim.SetActiveBack(true);
            playerAnim.SetActiveRight(false);
            playerAnim.SetActiveLeft(false);
        }
        else if (isMovingRight)
        {
            playerAnim.SetActiveBack(false);
            playerAnim.SetActiveRight(true);
            playerAnim.SetActiveLeft(false);
        }
        else if (isMovingLeft)
        {
            playerAnim.SetActiveBack(false);
            playerAnim.SetActiveRight(false);
            playerAnim.SetActiveLeft(true);
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
        _currentWeapon.StopShooting();
        onPlayerDieded?.Invoke();
    }

    #endregion

    private void OnDestroy()
    {
        Unsubscribed();
    }

    public void SetMoveSpeed(float speed) => settings.moveSpeed = speed;
}
