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
    [SerializeField] private float stateCooldownTimer;
    private float retreatTimer;
    private bool pendingRetreatAfterHit; 
    [SerializeField] private RangedState state = RangedState.Chase;

    public override void Initialize(Transform playerTransform)
    {
        base.Initialize(playerTransform);
        ValidateSettings();
        InitializeProjectilePool();

        navMeshAgent.speed = settings.moveSpeed;
        state = RangedState.Chase;
        stateCooldownTimer = 0f;
    }

    private void ValidateSettings()
    {
        if (settings.minAttackDistance <= settings.safeDistance)
            settings.minAttackDistance = settings.safeDistance + 1f;

        if (settings.panicDistance >= settings.safeDistance)
            settings.panicDistance = Mathf.Max(0.1f, settings.safeDistance * 0.4f);
    }

    public override void Respawn()
    {
        base.Respawn();
        navMeshAgent.speed = settings.moveSpeed;
        state = RangedState.Chase;
        stateCooldownTimer = 0f;
    }

    private void InitializeProjectilePool()
    {
        projectilePool.Clear();

        for (int i = 0; i < settings.initialPoolSize; i++)
        {
            CreatePooledProjectile();
        }
    }

    private void CreatePooledProjectile()
    {
        var projectile = Instantiate(projectilePrefab);
        projectile.SetActive(false);
        //projectile.transform.parent = shootPoint;

        var projectileComponent = projectile.GetComponent<Projectile>();
        projectileComponent.onPlayerDamaged += PlayerDamaged;

        projectilePool.Add(projectile);
    }

    protected override void HandleMovement()
    {
        if (!CanProcessMovement()) return;
        if (isGameEnded) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if(stateCooldownTimer > 0f) stateCooldownTimer -= Time.deltaTime;
        else stateCooldownTimer = 0f;

        switch (state)
        {
            case RangedState.HitRetreat:
                HandleHitRetreatState(distanceToPlayer);
                break;

            case RangedState.Flee:
                HandleFleeState(distanceToPlayer);
                break;

            case RangedState.Shoot:
                HandleShootState(distanceToPlayer);
                break;

            case RangedState.Chase:
                HandleChaseState(distanceToPlayer);
                break;

            case RangedState.Dead:
                HandleDeadState();
                break;
        }

        UpdateRunAnimation();
    }

    private bool CanProcessMovement()
    {
        if (player == null || !isAlive)
        {
            state = isAlive ? state : RangedState.Dead;
            return false;
        }
        return true;
    }

    private void HandleHitRetreatState(float distanceToPlayer)
    {
        enemyAnimate.DisableAnim(EnemyAnim.Attack);
        RetreatFromPlayer(settings.retreatStep * 1.25f);

        retreatTimer -= Time.deltaTime;

        if (retreatTimer <= 0f && stateCooldownTimer <= 0f)
        {
            state = DecideStateByDistance(distanceToPlayer);
            stateCooldownTimer = settings.stateChangeCooldown;
        }
    }

    private void HandleFleeState(float distanceToPlayer)
    {
        enemyAnimate.DisableAnim(EnemyAnim.Attack);
        navMeshAgent.speed = settings.panicSpeed;

        UpdatePanicTarget();
        MoveToPanicTarget();

        if (distanceToPlayer > settings.safeDistance && stateCooldownTimer <= 0f)
        {
            state = RangedState.Shoot;
            stateCooldownTimer = settings.stateChangeCooldown;
        }
    }

    private void UpdatePanicTarget()
    {
        panicRecalcTimer -= Time.deltaTime;

        if (panicRecalcTimer <= 0f || Vector3.Distance(transform.position, panicTarget) < 1f)
        {
            panicTarget = GetRandomPanicPoint();
            panicRecalcTimer = settings.panicRecalcTime;

            if (panicTarget == Vector3.zero)
            {
                state = RangedState.Shoot;
                stateCooldownTimer = settings.stateChangeCooldown;
            }
        }
    }

    private void MoveToPanicTarget()
    {
        if (navMeshAgent.isActiveAndEnabled && navMeshAgent.isOnNavMesh)
            navMeshAgent.SetDestination(panicTarget);
    }

    private void HandleShootState(float distanceToPlayer)
    {
        navMeshAgent.speed = settings.moveSpeed;

        if (distanceToPlayer <= settings.panicDistance)
        {
            state = RangedState.Flee;
            stateCooldownTimer = settings.stateChangeCooldown;
            return;
        }

        if (distanceToPlayer > settings.minAttackDistance && stateCooldownTimer <= 0f)
        {
            state = RangedState.Chase;
            stateCooldownTimer = settings.stateChangeCooldown;
            return;
        }

        StopAgent();
        LookAtPlayer();
        TryAttack();
    }

    private void HandleChaseState(float distanceToPlayer)
    {
        enemyAnimate.DisableAnim(EnemyAnim.Attack);
        navMeshAgent.speed = settings.moveSpeed;

        if (distanceToPlayer <= settings.safeDistance && stateCooldownTimer <= 0f)
        {
            state = RangedState.Flee;
            stateCooldownTimer = settings.stateChangeCooldown;
        }
        else if (distanceToPlayer > settings.safeDistance &&
                 distanceToPlayer <= settings.minAttackDistance &&
                 stateCooldownTimer <= 0f)
        {
            state = RangedState.Shoot;
            stateCooldownTimer = settings.stateChangeCooldown;
        }
        else
        {
            ApproachPlayer();
        }
    }

    private void HandleDeadState()
    {
        StopAgent();
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

        Vector3 awayDirection = (transform.position - player.position).normalized;
        Vector3 retreatTarget = transform.position + awayDirection * Mathf.Max(2f, step);
        navMeshAgent.SetDestination(retreatTarget);
    }

    private void ApproachPlayer()
    {
        if (!navMeshAgent.isActiveAndEnabled || !navMeshAgent.isOnNavMesh) return;
        navMeshAgent.SetDestination(player.position);
    }

    private void LookAtPlayer()
    {
        Vector3 direction = (player.position - transform.position);
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(direction.normalized);
    }

    private RangedState DecideStateByDistance(float distance)
    {
        if (distance <= settings.safeDistance) return RangedState.Flee;
        if (distance <= settings.minAttackDistance) return RangedState.Shoot;
        return RangedState.Chase;
    }

    private void UpdateRunAnimation()
    {
        bool shouldRun = isAlive &&
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

        Vector3 direction = (player.position - shootPoint.position).normalized;

        var projectileComponent = projectile.GetComponent<Projectile>();
        projectileComponent.Launch(direction, settings.projectileSpeed, settings.maxProjectileDistance);
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
                return projectile;
        }

        return CreateNewProjectile();
    }

    private GameObject CreateNewProjectile()
    {
        GameObject newProjectile = Instantiate(projectilePrefab);
        //newProjectile.transform.parent = shootPoint;
        newProjectile.SetActive(false);

        var projectileComponent = newProjectile.GetComponent<Projectile>();
        projectileComponent.onPlayerDamaged += PlayerDamaged;

        projectilePool.Add(newProjectile);
        return newProjectile;
    }

    public override void TakeDamage(float damage, Collider hitTrigger = null)
    {
        if (!isAlive) return;

        float finalDamage = damage * GetDamageMultiplier(hitTrigger);
        currentHealth -= finalDamage;
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
            state = RangedState.HitRetreat;
            retreatTimer = settings.retreatDuration;
            navMeshAgent.speed = settings.panicSpeed;
            stateCooldownTimer = settings.stateChangeCooldown;
            pendingRetreatAfterHit = false;
        }
    }

    protected override void Die()
    {
        base.Die();
        state = RangedState.Dead;

        StopAgent();

        //if (navMeshAgent != null)
            //navMeshAgent.enabled = false;

        enemyAnimate.PlayDieAndLock();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    protected override bool IsHeadshot(Collider hitTrigger) => false;
    protected override bool IsLimb(Collider hitTrigger) => false;

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Safe zone radius
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, settings.safeDistance);

        // Attack zone radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, settings.minAttackDistance);

        // Panic zone radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, settings.panicDistance);
    }
#endif
}
