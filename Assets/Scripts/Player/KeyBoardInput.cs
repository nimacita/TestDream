using UnityEngine;

public class KeyBoardInput : IInputProvider
{
    public Vector3 GetMovementInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        return new Vector2(horizontal, vertical).normalized;
    }

    public Vector2 GetMouseInput() => new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
    public bool IsInputActive() => true;

    public Vector2 GetTouchDelta()
    {
        return Vector2.zero;
    }

    public bool GetShootInput()
    {
        return Input.GetMouseButton(0);
    }

    public bool GetReloadInput()
    {
        return Input.GetKeyDown(KeyCode.R);
    }
}
