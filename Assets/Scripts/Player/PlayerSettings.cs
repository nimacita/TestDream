using UnityEngine;

[CreateAssetMenu(menuName = "Player/Player Settings")]
public class PlayerSettings : ScriptableObject
{
    [Header("Health Settings")]
    public float startHealth = 100;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Rotation Settings")]
    public float mouseSensitivity = 2f;
    public float minVerticalAngle = -15f; // Максимальный угол вниз
    public float maxVerticalAngle = 20f;  // Максимальный угол вверх

    [Header("Invert Settings")]
    public bool invertY = true; // Инвертировать ось Y
    public bool invertX = false; // Инвертировать ось X

    [Header("Smooth Settings")]
    public float horizontalSmoothSpeed = 10f;
    public bool useHorizontalSmoothing = true;
    public float verticalSmoothSpeed = 10f;
    public bool useVerticalSmoothing = true;
}
