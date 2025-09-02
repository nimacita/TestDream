using UnityEngine;

public class JoystickInput : IInputProvider
{
    private Joystick _joystick;
    private int _touchId = -1;
    private Vector2 _touchStartPosition;
    private Vector2 _previousTouchPosition;
    private bool _isTouching = false;
    private float _touchSensitivity = 0.2f;

    public JoystickInput(Joystick joystick)
    {
        _joystick = joystick;
    }

    public Vector3 GetMovementInput()
    {
        return _joystick.Direction;
    }

    public Vector2 GetMouseInput()
    {
        return Vector2.zero;
    }

    public Vector2 GetTouchDelta()
    {
        if (!_isTouching) return Vector2.zero;

        foreach (Touch touch in Input.touches)
        {
            if (touch.fingerId == _touchId)
            {
                Vector2 delta = touch.deltaPosition * _touchSensitivity * Time.deltaTime;
                _previousTouchPosition = touch.position;
                return delta;
            }
        }

        return Vector2.zero;
    }

    public bool IsInputActive()
    {
        UpdateTouchState();
        return _isTouching || _joystick.Direction.magnitude > 0.1f;
    }

    private void UpdateTouchState()
    {
        foreach (Touch touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Began && !IsTouchOnJoystick(touch.position))
            {
                _touchId = touch.fingerId;
                _touchStartPosition = touch.position;
                _previousTouchPosition = touch.position;
                _isTouching = true;
                return;
            }
        }

        if (_isTouching)
        {
            foreach (Touch touch in Input.touches)
            {
                if (touch.fingerId == _touchId &&
                   (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled))
                {
                    _isTouching = false;
                    _touchId = -1;
                    return;
                }
            }
        }
    }

    private bool IsTouchOnJoystick(Vector2 touchPosition)
    {
        if (_joystick == null) return false;

        RectTransform joystickRect = _joystick.GetComponent<RectTransform>();
        Vector2 joystickScreenPos = RectTransformUtility.WorldToScreenPoint(
            null, joystickRect.position);

        float joystickRadius = joystickRect.sizeDelta.x * 0.5f;
        float distance = Vector2.Distance(touchPosition, joystickScreenPos);

        return distance <= joystickRadius * 2f; 
    }

    public bool GetShootInput()
    {
        // Реализуйте touch-контролы для стрельбы
        return false;
    }

    public bool GetReloadInput()
    {
        // Реализуйте touch-контролы для перезарядки
        return false;
    }
}
