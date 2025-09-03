using UnityEngine;
using Utilities.Enums;

[CreateAssetMenu(menuName = "Enemys/Enemy Settings")]
public class EnemySettings : ScriptableObject
{
    //Main
    [Header("Main Settings")]
    public string enemyName;
    public EnemyType enemyType;

    [Header("Movement Settings")]
    public float moveSpeed = 3.5f;
    public float stoppingDistance = 1.5f;
    public float acceleration = 8f;
    public float angularSpeed = 360f;

    [Header("Health Settings")]
    public float startHealth = 100;

    [Header("Attack Settings")]
    public int attackDamage = 10;

    //Melee
    [Header("Melle Attack Settings")]
    public float attackRange = 3f;
    public float attackCooldown = 1f;
    public float attackWindupTime = 0.5f;

    //Range
    [Header("Range Attack Settings")]
    [Tooltip("Комфортная дистанцуия от игрока")]
    public float safeDistance = 15f;        
    [Tooltip("Дистанция трельбы")]
    public float minAttackDistance = 20f;     
    [Tooltip("Выстрелы в секунду")]
    public float fireRate = 0.5f;
    public int initialPoolSize = 3;         

    [Header("Projectile Settings")]
    public float projectileSpeed = 30f;
    public float maxProjectileDistance = 50f;

    [Header("Panic Settings")]
    [Tooltip("Дистанция до игрока для паники")]
    public float panicDistance = 6f;         
    [Tooltip("Радиус паники")]
    public float panicMoveRadius = 6f;  
    [Tooltip("Частота смены поожения")]
    public float panicRecalcTime = 1f;   
    [Tooltip("Попытки найти точку")]
    public int panicPointSearchAttempts = 10; 
    [Tooltip("Скорость бега в мпанике")]
    public float panicSpeed = 6f;           

    [Header("Retreat Settings")]
    [Tooltip("Сколько убегаем после урона")]
    public float retreatDuration = 2f;     
    [Tooltip("Длина убегания")]
    public float retreatStep = 4f;            
    [Tooltip("задержка между состоянияеми")]
    public float stateChangeCooldown = 0.25f; 

    //Main
    [Header("Damage Multipliers")]
    public float headshotMultiplier = 2f;
    public float bodyshotMultiplier = 1f;
    public float limbMultiplier = 0.7f;

    [Header("Dead Settings")]
    [Tooltip("Время которое труп лежит после смерти до исчезновения")]
    public float dieDisabledtime = 8f;
}
