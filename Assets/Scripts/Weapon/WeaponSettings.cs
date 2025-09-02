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
    [Tooltip("Название оружия для идентификации")]
    public string weaponName = "New Weapon";
    [Tooltip("Режим стрельбы: Single - одиночные, Auto - автоматический, Burst - очередью")]
    public FireMode fireMode = FireMode.Single;
    [Tooltip("Урон за один выстрел")]
    public float damage = 10f;
    [Tooltip("Скорострельность (выстрелов в секунду)")]
    public float fireRate = 5f;
    [Tooltip("Максимальное количество патронов в обойме")]
    public int maxAmmo = 30;
    [Tooltip("Время перезарядки в секундах")]
    public float reloadTime = 3f;
    [Tooltip("Количество выстрелов в очереди (только для режима Burst)")]
    public int burstCount = 3;
    [Tooltip("Задержка между выстрелами в очереди (только для режима Burst)")]
    public float burstDelay = 0.1f;

    [Header("Spread Settings")]
    [Tooltip("Угол разброса в градусах (0 = идеальная точность)")]
    public float spreadAngle = 2f;
    [Tooltip("Максимальный разброс при автоматической стрельбе")]
    public float maxSpreadAngle = 10f;
    [Tooltip("Скорость увеличения разброса за выстрел (только для Auto)")]
    public float spreadIncreasePerShot = 0.5f;
    [Tooltip("Скорость восстановления точности в секунду")]
    public float spreadRecoveryRate = 2f;

    [Header("Visual & Audio")]
    [Tooltip("Звук выстрела")]
    public AudioClip shootSound;
    [Tooltip("Звук перезарядки")]
    public AudioClip reloadSound;
}
