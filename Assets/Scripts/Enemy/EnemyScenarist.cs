using UnityEngine;
using System;
using Unity.AI.Navigation;
using System.Collections.Generic;
using System.Collections;

[Serializable]
public class EnemyObject
{
    public GameObject enemyPrefab;
    public EnemyBase enemyBase;
    [Range(0f,1f)]
    public float spawnChance;
}

public class EnemyScenarist : InitializedBehaviour
{
    [Header("Enemys")]
    [SerializeField] private EnemyObject[] allEnemysType;
    private List<EnemyBase> enemyPool = new List<EnemyBase>();

    [Header("Settings")]
    [SerializeField] private int startEnemyCount = 5;
    [SerializeField] private int maxEnemyCount = 20;
    [SerializeField] private Vector2 enemySpawnSpeed = new Vector2(1f, 3f);
    [SerializeField] private float minDistanceFromPlayer = 10;

    [Header("Components")]
    [SerializeField] private Transform enemyHolder;
    [SerializeField] private GameObject floor;
    private NavMeshSurface floorSurface;
    private PlayerController _playerController;
    private Transform _playerTr;

    private bool spawningActive = false;

    public override void Entry(params object[] dependencies)
    {
        _playerController = GetDependency<PlayerController>(dependencies);
        _playerTr = _playerController.gameObject.transform;
        floorSurface = floor.GetComponent<NavMeshSurface>();

        StartEnemySpawn();
    }

    private void StartEnemySpawn()
    {
        spawningActive = true;

        for (int i = 0; i < startEnemyCount; i++)
        {
            SpawnEnemy();
        }

        StartCoroutine(EnemySpawnRoutine());
    }

    private IEnumerator EnemySpawnRoutine()
    {
        while (spawningActive)
        {
            int activeCount = enemyPool.FindAll(e => e.gameObject.activeSelf).Count;
            if (activeCount < maxEnemyCount)
            {
                SpawnEnemy();
            }

            float delay = UnityEngine.Random.Range(enemySpawnSpeed.x, enemySpawnSpeed.y);
            yield return new WaitForSeconds(delay);
        }
    }

    private void SpawnEnemy()
    {
        EnemyObject enemyType = GetRandomEnemyType();
        if (enemyType == null) return;

        EnemyBase enemy = null;
        foreach (var pooledEnemy in enemyPool)
        {
            if (enemyType.enemyBase.Settings == pooledEnemy.Settings
                && !pooledEnemy.gameObject.activeSelf)
            {
                enemy = pooledEnemy;
            }
        }

        if (enemy == null)
        {
            Vector3 spawnPos = GetRandomNavmeshPoint();
            GameObject enemyObj = Instantiate(enemyType.enemyPrefab, spawnPos, Quaternion.identity, enemyHolder);
            enemy = enemyObj.GetComponent<EnemyBase>();
            enemy.Initialize(_playerTr);
            enemyPool.Add(enemy);
        }
        else
        {
            enemy.transform.position = GetRandomNavmeshPoint();
            enemy.gameObject.SetActive(true);
            enemy.Respawn();
        }
    }

    private EnemyObject GetRandomEnemyType()
    {
        float totalChance = 0;
        foreach (var enemy in allEnemysType) totalChance += enemy.spawnChance;

        float randomPoint = UnityEngine.Random.value * totalChance;
        float current = 0;

        foreach (var enemy in allEnemysType)
        {
            current += enemy.spawnChance;
            if (randomPoint <= current)
                return enemy;
        }
        return null;
    }

    private Vector3 GetRandomNavmeshPoint()
    {
        Vector2 randomDirection2D = UnityEngine.Random.insideUnitCircle.normalized;
        Vector3 randomDirection = new Vector3(randomDirection2D.x, 0f, randomDirection2D.y);

        float distance = UnityEngine.Random.Range(minDistanceFromPlayer, minDistanceFromPlayer * 2f);

        Vector3 potentialPoint = _playerTr.position + randomDirection * distance;
        potentialPoint.y = floor.transform.position.y + 1f;

        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(potentialPoint, out hit, 2f, UnityEngine.AI.NavMesh.AllAreas))
        {
            return hit.position;
        }

        return GetRandomNavmeshPoint();
    }

    public void GameEnd()
    {
        spawningActive = false;
        foreach (var enemy in enemyPool)
        {
            enemy.GameEnd();
        }
    }
}
