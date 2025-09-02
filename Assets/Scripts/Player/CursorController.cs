using UnityEngine;

public class CursorController: InitializedBehaviour
{
    [Header("Cursor Settings")]
    private bool hideCursorWhenLocked = true;
    private bool isCursorLocked = true;

    public override void Entry(params object[] dependencies)
    {
        LockCursor();
    }

    public void ToggleCursor()
    {
        isCursorLocked = !isCursorLocked;
        UpdateCursorState();
    }

    public void LockCursor()
    {
        isCursorLocked = true;
        UpdateCursorState();
    }

    public void UnlockCursor()
    {
        isCursorLocked = false;
        UpdateCursorState();
    }

    private void UpdateCursorState()
    {
        if (isCursorLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = !hideCursorWhenLocked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // Для внешнего управления
    public void SetCursorState(bool locked)
    {
        isCursorLocked = locked;
        UpdateCursorState();
    }

    public bool IsCursorLocked() => isCursorLocked;
}
