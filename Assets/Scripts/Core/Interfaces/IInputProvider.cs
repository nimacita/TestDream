using UnityEngine;

public interface IInputProvider
{
    Vector3 GetMovementInput();
    Vector2 GetMouseInput();
    bool IsInputActive();
    Vector2 GetTouchDelta();
    bool GetShootInput();
    bool GetReloadInput();
}
