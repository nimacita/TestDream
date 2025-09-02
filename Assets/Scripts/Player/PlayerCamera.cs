using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float _mouseSensitivity = 2f;
    [SerializeField] private float _minVerticalAngle = -15f; // ������������ ���� ����
    [SerializeField] private float _maxVerticalAngle = 20f;  // ������������ ���� �����
    [SerializeField] private bool _invertY = false; // ������������� ��� Y

    [Header("Horizontal Rotation")]
    [SerializeField] private bool _invertX = false; // ������������� ��� X
    [SerializeField] private float _horizontalSmoothSpeed = 10f;
    [SerializeField] private bool _useHorizontalSmoothing = true;

    [Header("Smoothing")]
    [SerializeField] private float _verticalSmoothSpeed = 10f;
    [SerializeField] private bool _useVerticalSmoothing = true;

    private bool _isCameraInit = false;
    private float _targetRotationX = 0f;
    private float _currentRotationX = 0f;
    private float _targetRotationY = 0f;
    private float _currentRotationY = 0f;
    private IInputProvider _inputProvider;
    private Transform _playerTransform;

    public void Init(IInputProvider inputProvider, Transform playerTransform)
    {
        _playerTransform = playerTransform;

        // �������������� ������� ���� ��������
        _currentRotationX = transform.localEulerAngles.x;
        _targetRotationX = _currentRotationX;
        _currentRotationY = _playerTransform.eulerAngles.y;
        _targetRotationY = _currentRotationY;

        // ����������� ���� � �������� -180 �� 180
        if (_currentRotationX > 180f)
            _currentRotationX -= 360f;
        if (_currentRotationY > 180f)
            _currentRotationY -= 360f;

        // �������� ��������� �����
        _inputProvider = inputProvider;

        _isCameraInit = true;
    }

    private void LateUpdate()
    {
        if (_inputProvider == null || !_isCameraInit) return;

        HandleInput();
        UpdateCameraRotation();
    }

    private void HandleInput()
    {
        Vector2 mouseInput = _inputProvider.GetMouseInput();

        // ������������ ��� Y (������������ �������� ����)
        if (Mathf.Abs(mouseInput.y) > 0.01f)
        {
            float verticalInput = _invertY ? -mouseInput.y : mouseInput.y;
            _targetRotationX += verticalInput * _mouseSensitivity;

            // ������������ ���� �������� �� ���������
            _targetRotationX = Mathf.Clamp(_targetRotationX, _minVerticalAngle, _maxVerticalAngle);
        }

        // ������������ ��� X (�������������� �������� ����)
        if (Mathf.Abs(mouseInput.x) > 0.01f)
        {
            float horizontalInput = _invertX ? -mouseInput.x : mouseInput.x;
            _targetRotationY += horizontalInput * _mouseSensitivity;
        }
    }

    private void UpdateCameraRotation()
    {
        // ������� �������� �� ��������� (������)
        if (_useVerticalSmoothing)
        {
            _currentRotationX = Mathf.Lerp(_currentRotationX, _targetRotationX,
                _verticalSmoothSpeed * Time.deltaTime);
        }
        else
        {
            _currentRotationX = _targetRotationX;
        }

        // ������� �������� �� ����������� (�����)
        if (_useHorizontalSmoothing)
        {
            _currentRotationY = Mathf.Lerp(_currentRotationY, _targetRotationY,
                _horizontalSmoothSpeed * Time.deltaTime);
        }
        else
        {
            _currentRotationY = _targetRotationY;
        }

        // ��������� ������������ �������� � ������
        transform.localEulerAngles = new Vector3(_currentRotationX, 0f, 0f);

        // ��������� �������������� �������� � ������
        _playerTransform.eulerAngles = new Vector3(0f, _currentRotationY, 0f);
    }

    // ������ ��� �������� ����������
    public void SetVerticalAngle(float angle)
    {
        _targetRotationX = Mathf.Clamp(angle, _minVerticalAngle, _maxVerticalAngle);
        if (!_useVerticalSmoothing)
            _currentRotationX = _targetRotationX;
    }

    public void SetHorizontalAngle(float angle)
    {
        _targetRotationY = angle;
        if (!_useHorizontalSmoothing)
            _currentRotationY = _targetRotationY;
    }

    public void ResetCamera()
    {
        _targetRotationX = 0f;
        _targetRotationY = _playerTransform.eulerAngles.y;
        if (!_useVerticalSmoothing)
            _currentRotationX = 0f;
        if (!_useHorizontalSmoothing)
            _currentRotationY = _targetRotationY;
    }

    public void SetSensitivity(float sensitivity) => _mouseSensitivity = sensitivity;
    public void SetVerticalLimits(float min, float max)
    {
        _minVerticalAngle = min;
        _maxVerticalAngle = max;
        _targetRotationX = Mathf.Clamp(_targetRotationX, _minVerticalAngle, _maxVerticalAngle);
    }

    public float GetCurrentVerticalAngle() => _currentRotationX;
    public float GetTargetVerticalAngle() => _targetRotationX;
    public float GetCurrentHorizontalAngle() => _currentRotationY;
    public float GetTargetHorizontalAngle() => _targetRotationY;
}
