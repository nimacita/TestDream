using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    private bool _isCameraInit = false;
    private bool _isDied;
    private float _targetRotationX = 0f;
    private float _currentRotationX = 0f;
    private float _targetRotationY = 0f;
    private float _currentRotationY = 0f;
    private IInputProvider _inputProvider;
    private Transform _playerTransform;
    private PlayerSettings settings;

    public void Init(IInputProvider inputProvider, Transform playerTransform, PlayerSettings playerSettings)
    {
        settings = playerSettings;
        _playerTransform = playerTransform;

        _currentRotationX = transform.localEulerAngles.x;
        _targetRotationX = _currentRotationX;
        _currentRotationY = _playerTransform.eulerAngles.y;
        _targetRotationY = _currentRotationY;

        if (_currentRotationX > 180f)
            _currentRotationX -= 360f;
        if (_currentRotationY > 180f)
            _currentRotationY -= 360f;

        _inputProvider = inputProvider;

        _isCameraInit = true;
        _isDied = false;
    }

    private void LateUpdate()
    {
        if (_inputProvider == null || !_isCameraInit || _isDied) return;

        HandleInput();
        UpdateCameraRotation();
    }

    private void HandleInput()
    {
        Vector2 mouseInput = _inputProvider.GetMouseInput();

        if (Mathf.Abs(mouseInput.y) > 0.01f)
        {
            float verticalInput = settings.invertY ? -mouseInput.y : mouseInput.y;
            _targetRotationX += verticalInput * settings.mouseSensitivity;

            _targetRotationX = Mathf.Clamp(_targetRotationX, settings.minVerticalAngle, settings.maxVerticalAngle);
        }

        if (Mathf.Abs(mouseInput.x) > 0.01f)
        {
            float horizontalInput = settings.invertX ? -mouseInput.x : mouseInput.x;
            _targetRotationY += horizontalInput * settings.mouseSensitivity;
        }
    }

    private void UpdateCameraRotation()
    {
        if (settings.useVerticalSmoothing)
        {
            _currentRotationX = Mathf.Lerp(_currentRotationX, _targetRotationX,
                settings.verticalSmoothSpeed * Time.deltaTime);
        }
        else
        {
            _currentRotationX = _targetRotationX;
        }

        if (settings.useHorizontalSmoothing)
        {
            _currentRotationY = Mathf.Lerp(_currentRotationY, _targetRotationY,
                settings.horizontalSmoothSpeed * Time.deltaTime);
        }
        else
        {
            _currentRotationY = _targetRotationY;
        }

        transform.localEulerAngles = new Vector3(_currentRotationX, 0f, 0f);

        _playerTransform.eulerAngles = new Vector3(0f, _currentRotationY, 0f);
    }

    public void SetVerticalAngle(float angle)
    {
        _targetRotationX = Mathf.Clamp(angle, settings.minVerticalAngle, settings.maxVerticalAngle);
        if (!settings.useVerticalSmoothing)
            _currentRotationX = _targetRotationX;
    }

    public void SetHorizontalAngle(float angle)
    {
        _targetRotationY = angle;
        if (!settings.useHorizontalSmoothing)
            _currentRotationY = _targetRotationY;
    }

    public void ResetCamera()
    {
        _targetRotationX = 0f;
        _targetRotationY = _playerTransform.eulerAngles.y;
        if (!settings.useVerticalSmoothing)
            _currentRotationX = 0f;
        if (!settings.useHorizontalSmoothing)
            _currentRotationY = _targetRotationY;
    }

    public void SetSensitivity(float sensitivity) => settings.mouseSensitivity = sensitivity;
    public void SetVerticalLimits(float min, float max)
    {
        settings.minVerticalAngle = min;
        settings.maxVerticalAngle = max;
        _targetRotationX = Mathf.Clamp(_targetRotationX, settings.minVerticalAngle, settings.maxVerticalAngle);
    }

    public void PlayerDied()
    {
        _isDied = true;
    }

    public float GetCurrentVerticalAngle() => _currentRotationX;
    public float GetTargetVerticalAngle() => _targetRotationX;
    public float GetCurrentHorizontalAngle() => _currentRotationY;
    public float GetTargetHorizontalAngle() => _targetRotationY;
}
