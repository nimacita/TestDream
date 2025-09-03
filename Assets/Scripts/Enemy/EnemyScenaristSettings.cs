using UnityEngine;

[CreateAssetMenu(menuName = "Enemys/Enemy Scenarist Settings")]
public class EnemyScenaristSettings : ScriptableObject
{
    [Header("Enemys")]
    public EnemyObject[] allEnemysType;

    [Header("Settings")]
    public int startEnemyCount = 5;
    public int maxEnemyCount = 20;
    public Vector2 enemySpawnSpeedRange = new Vector2(1f, 3f);
    public float minDistanceFromPlayer = 10;
}
