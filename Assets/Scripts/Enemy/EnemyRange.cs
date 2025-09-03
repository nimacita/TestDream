using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Utilities.Enums;

public class EnemyRange : EnemyBase
{
    [Header("Components")]
    [SerializeField] private Transform shootPoint;              
    [SerializeField] private GameObject projectilePrefab;       

    [Header("Settings")]
    private float panicRecalcTimer;                     
    private Vector3 panicTarget;
    private readonly List<GameObject> projectilePool = new();
    private float lastShotTime;
    private float stateCooldownTimer;
    private float retreatTimer;
    private bool pendingRetreatAfterHit; 
    private RangedState state = RangedState.Chase;

    public override void Initialize(Transform playerTransform)
    {
        base.Initialize(playerTransform);

        if (settings.minAttackDistance <= settings.safeDistance) settings.minAttackDistance = settings.safeDistance + 1f;
        if (settings.panicDistance >= settings.safeDistance) settings.panicDistance = Mathf.Max(0.1f, settings.safeDistance * 0.4f);

        InitializeProjectilePool();

        navMeshAgent.speed = settings.moveSpeed;
        state = RangedState.Chase;
        stateCooldownTimer = 0f;
    }

    private void InitializeProjectilePool()
    {
        projectilePool.Clear();
        for (int i = 0; i < settings.initialPoolSize; i++)
        {
            var proj = Instantiate(projectilePrefab);
            proj.SetActive(false);
            proj.transform.parent = shootPoint;
            proj.GetComponent<Projectile>().onPlayerDamaged += PlayerDamaged;
            projectilePool.Add(proj);
        }
    }

    protected override void HandleMovement()
    {
        if (player == null || !isAlive)
        {
            state = isAlive ? state : RangedState.Dead;
            return;
        }

        float dist = Vector3.Distance(transform.position, player.position);
        stateCooldownTimer -= Time.deltaTime;

        switch (state)
        {
            case RangedState.HitRetreat:
                enemyAnimate.DisableAnim(EnemyAnim.Attack);
                RetreatFromPlayer(settings.retreatStep * 1.25f);
                retreatTimer -= Time.deltaTime;
                if (retreatTimer <= 0f && stateCooldownTimer <= 0f)
                {
                    state = DecideStateByDistance(dist);
                    stateCooldownTimer = settings.stateChangeCooldown;
                }
                break;

            case RangedState.Flee:
                enemyAnimate.DisableAnim(EnemyAnim.Attack);
                navMeshAgent.speed = settings.panicSpeed;

                panicRecalcTimer -= Time.deltaTime;
                if (panicRecalcTimer <= 0f || Vector3.Distance(transform.position, panicTarget) < 1f)
                {
                    panicTarget = GetRandomPanicPoint();
                    panicRecalcTimer = settings.panicRecalcTime;

                    if (panicTarget == Vector3.zero)
                    {
                        state = RangedState.Shoot;
                        stateCooldownTimer = settings.stateChangeCooldown;
                        break;
                    }
                }

                if (navMeshAgent.isActiveAndEnabled && navMeshAgent.isOnNavMesh)
                    navMeshAgent.SetDestination(panicTarget);

                if (dist > settings.safeDistance && stateCooldownTimer <= 0f)
                {
                    state = RangedState.Shoot;
                    stateCooldownTimer = settings.stateChangeCooldown;
                }
                break;

            case RangedState.Shoot:
                navMeshAgent.speed = settings.moveSpeed;
                if (dist <= settings.panicDistance)
                {
                    state = RangedState.Flee;
                    stateCooldownTimer = settings.stateChangeCooldown;
                    break;
                }
                if (dist > settings.minAttackDistance && stateCooldownTimer <= 0f)
                {
                    state = RangedState.Chase;
                    stateCooldownTimer = settings.stateChangeCooldown;
                    break;
                }

                StopAgent();
                LookAtPlayer();
                TryAttack(); 
                break;

            case RangedState.Chase:
                enemyAnimate.DisableAnim(EnemyAnim.Attack);
                navMeshAgent.speed = settings.moveSpeed;
                if (dist <= settings.safeDistance && stateCooldownTimer <= 0f)
                {
                    state = RangedState.Flee;
                    stateCooldownTimer = settings.stateChangeCooldown;
                }
                else if (dist > settings.safeDistance && dist <= settings.minAttackDistance && stateCooldownTimer <= 0f)
                {
                    state = RangedState.Shoot;
                    stateCooldownTimer = settings.stateChangeCooldown;
                }
                else
                {
                    ApproachPlayer();
                }
                break;

            case RangedState.Dead:
                StopAgent();
                return;
        }

        UpdateRunAnimation();
    }

    protected override void IsMoving()
    {
        if (!navMeshAgent.isActiveAndEnabled || !navMeshAgent.isOnNavMesh || !isAlive)
        {
            isMove = false;
            StopAgent();
            return;
        }

        if (state == RangedState.Shoot || isTakingDamage)
        {
            isMove = false;
            StopAgent();
            return;
        }

        isMove = true;
        navMeshAgent.isStopped = false;
    }

    private Vector3 GetRandomPanicPoint()
    {
        if (player == null)
            return transform.position;

        Vector3 awayFromPlayer = (transform.position - player.position).normalized;

        for (int i = 0; i < settings.panicPointSearchAttempts; i++)
        {
            Vector3 randomDir = Random.insideUnitSphere;
            randomDir.y = 0;
            randomDir.Normalize();

            if (Vector3.Dot(randomDir, awayFromPlayer) < 0f)
                continue;

            Vector3 candidate = transform.position + randomDir * Random.Range(2f, settings.panicMoveRadius);

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, settings.panicMoveRadius, NavMesh.AllAreas))
                return hit.position;
        }

        return Vector3.zero;
    }

    private void StopAgent()
    {
        if (navMeshAgent.isActiveAndEnabled)
            navMeshAgent.isStopped = true;
    }

    private void RetreatFromPlayer(float step)
    {
        if (!navMeshAgent.isActiveAndEnabled || !navMeshAgent.isOnNavMesh) return;

        Vector3 away = (transform.position - player.position).normalized;
        Vector3 target = transform.position + away * Mathf.Max(2f, step);
        navMeshAgent.SetDestination(target);
    }

    private void ApproachPlayer()
    {
        if (!navMeshAgent.isActiveAndEnabled || !navMeshAgent.isOnNavMesh) return;
        navMeshAgent.SetDestination(player.position);
    }

    private void LookAtPlayer()
    {
        Vector3 dir = (player.position - transform.position);
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(dir.normalized);
    }

    private RangedState DecideStateByDistance(float dist)
    {
        if (dist <= settings.safeDistance) return RangedState.Flee;
        if (dist <= settings.minAttackDistance) return RangedState.Shoot;
        return RangedState.Chase;
    }

    private void UpdateRunAnimation()
    {
        bool shouldRun =
            isAlive &&
            !isTakingDamage &&
            state != RangedState.Shoot &&
            state != RangedState.Dead &&
            navMeshAgent.isActiveAndEnabled;

        enemyAnimate.SetRun(shouldRun);
    }

    protected override void TryAttack()
    {
        if (Time.time - lastShotTime < 1f / settings.fireRate) return;

        isAttacking = true;         
        enemyAnimate.EnableAnim(EnemyAnim.Attack);
        lastShotTime = Time.time;
    }

    protected override void PerformAttack()
    {
        ShootProjectile();
        isAttacking = false;
    }

    private void ShootProjectile()
    {
        GameObject projectile = GetProjectileFromPool();
        projectile.transform.position = shootPoint.position;
        projectile.gameObject.SetActive(true);

        Vector3 dir = (player.position - shootPoint.position).normalized;

        var proj = projectile.GetComponent<Projectile>();
        proj.Launch(dir, settings.projectileSpeed, settings.maxProjectileDistance);
    }

    private void PlayerDamaged()
    {
        onPlayerDamaged?.Invoke(settings.attackDamage);
    }

    private GameObject GetProjectileFromPool()
    {
        foreach (var projectile in projectilePool)
        {
            if (!projectile.activeInHierarchy)
            {
                return projectile;
            }
        }
        GameObject newProjectile = Instantiate(projectilePrefab);
        newProjectile.transform.parent = shootPoint;
        newProjectile.SetActive(false);
        newProjectile.GetComponent<Projectile>().onPlayerDamaged += PlayerDamaged;
        projectilePool.Add(newProjectile);
        return newProjectile;
    }

    public override void TakeDamage(float damage, Collider hitTrigger = null)
    {
        if (!isAlive) return;

        float final = damage * GetDamageMultiplier(hitTrigger);
        currentHealth -= final;
        enemyCanvas.ChangeHealth(currentHealth, settings.startHealth);

        if (currentHealth <= 0f)
        {
            Die();
            return;
        }

        if (state == RangedState.Shoot)
        {
            pendingRetreatAfterHit = true; 
            InterruptAttack();
            PlayDamageAnimation();
        }
    }

    protected override void OnDamageAnimationEnd()
    {
        base.OnDamageAnimationEnd();

        if (!isAlive) return;

        if (pendingRetreatAfterHit)
        {
            pendingRetreatAfterHit = false;
            state = RangedState.HitRetreat;
            retreatTimer = settings.retreatDuration;
            navMeshAgent.speed = settings.panicSpeed;
            stateCooldownTimer = settings.stateChangeCooldown;
        }
    }

    protected override void Die()
    {
        base.Die();
        state = RangedState.Dead;

        StopAgent();
        if (navMeshAgent != null) navMeshAgent.enabled = false;
        enemyAnimate.PlayDieAndLock(); 
    }

    protected override void OnDestroy()
    {
        foreach (GameObject projectile in projectilePool)
        {
            projectile.GetComponent<Projectile>().onPlayerDamaged -= PlayerDamaged;
        }
        base.OnDestroy();
    }

    protected override bool IsHeadshot(Collider hitTrigger) => false;
    protected override bool IsLimb(Collider hitTrigger) => false;

#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        // Отображаем радиус безопасной зоны
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, settings.safeDistance);

        //зона атаки
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, settings.minAttackDistance);

        //зона паники
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, settings.panicDistance);
    }
#endif
}
