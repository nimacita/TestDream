using UnityEngine;

[CreateAssetMenu(menuName = "Enemys/Enemy Settings")]
public class EnemySettings : ScriptableObject
{
    [Header("Main SEttings")]
    public string enemyName;

    [Header("Movement Settings")]
    public float moveSpeed = 3.5f;
    public float stoppingDistance = 1.5f;
    public float acceleration = 8f;
    public float angularSpeed = 360f;

    [Header("Attack Settings")]
    public float attackRange = 3f;
    public float attackCooldown = 1f;
    public float attackWindupTime = 0.5f;
    public int attackDamage = 10;

    [Header("Health Settings")]
    public float startHealth = 100;

    [Header("Damage Multipliers")]
    public float headshotMultiplier = 2f;
    public float bodyshotMultiplier = 1f;
    public float limbMultiplier = 0.7f;
}
