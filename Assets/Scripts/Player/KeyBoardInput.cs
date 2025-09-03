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
    public Vector2 GetTouchDelta() => Vector2.zero;
    public bool GetShootInput() => Input.GetMouseButton(0);
    public bool GetReloadInput() => Input.GetKeyDown(KeyCode.R);

    public int? GetWeaponNumberInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) return 0;
        if (Input.GetKeyDown(KeyCode.Alpha2)) return 1;
        if (Input.GetKeyDown(KeyCode.Alpha3)) return 2;
        if (Input.GetKeyDown(KeyCode.Alpha4)) return 3;
        if (Input.GetKeyDown(KeyCode.Alpha5)) return 4;
        if (Input.GetKeyDown(KeyCode.Alpha6)) return 5;
        if (Input.GetKeyDown(KeyCode.Alpha7)) return 6;
        if (Input.GetKeyDown(KeyCode.Alpha8)) return 7;
        if (Input.GetKeyDown(KeyCode.Alpha9)) return 8;
        if (Input.GetKeyDown(KeyCode.Alpha0)) return 9;
        return null;
    }

    public float GetWeaponScrollInput()
    {
        return Input.GetAxis("Mouse ScrollWheel");
    }
}
