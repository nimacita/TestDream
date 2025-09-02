using UnityEngine;

public enum FireMode
{
    Single,    
    Auto,       
    Burst       
}

[CreateAssetMenu(menuName = "Weapons/Weapon Settings")]
public class WeaponSettings : ScriptableObject
{
    [Header("Basic Settings")]
    [Tooltip("�������� ������ ��� �������������")]
    public string weaponName = "New Weapon";
    [Tooltip("����� ��������: Single - ���������, Auto - ��������������, Burst - ��������")]
    public FireMode fireMode = FireMode.Single;
    [Tooltip("���� �� ���� �������")]
    public float damage = 10f;
    [Tooltip("���������������� (��������� � �������)")]
    public float fireRate = 5f;
    [Tooltip("������������ ���������� �������� � ������")]
    public int maxAmmo = 30;
    [Tooltip("����� ����������� � ��������")]
    public float reloadTime = 3f;
    [Tooltip("���������� ��������� � ������� (������ ��� ������ Burst)")]
    public int burstCount = 3;
    [Tooltip("�������� ����� ���������� � ������� (������ ��� ������ Burst)")]
    public float burstDelay = 0.1f;

    [Header("Spread Settings")]
    [Tooltip("���� �������� � �������� (0 = ��������� ��������)")]
    public float spreadAngle = 2f;
    [Tooltip("������������ ������� ��� �������������� ��������")]
    public float maxSpreadAngle = 10f;
    [Tooltip("�������� ���������� �������� �� ������� (������ ��� Auto)")]
    public float spreadIncreasePerShot = 0.5f;
    [Tooltip("�������� �������������� �������� � �������")]
    public float spreadRecoveryRate = 2f;

    [Header("Visual & Audio")]
    [Tooltip("���� ��������")]
    public AudioClip shootSound;
    [Tooltip("���� �����������")]
    public AudioClip reloadSound;
}
