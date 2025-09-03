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
    [Tooltip("���������� ���������� �� ������")]
    public float safeDistance = 15f;        
    [Tooltip("��������� �������")]
    public float minAttackDistance = 20f;     
    [Tooltip("�������� � �������")]
    public float fireRate = 0.5f;
    public int initialPoolSize = 3;         

    [Header("Projectile Settings")]
    public float projectileSpeed = 30f;
    public float maxProjectileDistance = 50f;

    [Header("Panic Settings")]
    [Tooltip("��������� �� ������ ��� ������")]
    public float panicDistance = 6f;         
    [Tooltip("������ ������")]
    public float panicMoveRadius = 6f;  
    [Tooltip("������� ����� ��������")]
    public float panicRecalcTime = 1f;   
    [Tooltip("������� ����� �����")]
    public int panicPointSearchAttempts = 10; 
    [Tooltip("�������� ���� � �������")]
    public float panicSpeed = 6f;           

    [Header("Retreat Settings")]
    [Tooltip("������� ������� ����� �����")]
    public float retreatDuration = 2f;     
    [Tooltip("����� ��������")]
    public float retreatStep = 4f;            
    [Tooltip("�������� ����� ������������")]
    public float stateChangeCooldown = 0.25f; 

    //Main
    [Header("Damage Multipliers")]
    public float headshotMultiplier = 2f;
    public float bodyshotMultiplier = 1f;
    public float limbMultiplier = 0.7f;

    [Header("Dead Settings")]
    [Tooltip("����� ������� ���� ����� ����� ������ �� ������������")]
    public float dieDisabledtime = 8f;
}
