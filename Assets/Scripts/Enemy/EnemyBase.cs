using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Utilities.Enums;

public abstract class EnemyBase : MonoBehaviour
{

    [Header("Enemy Settings")]
    [SerializeField] protected EnemySettings settings;

    [Header("Components")]
    [SerializeField] protected EnemyCanvas enemyCanvas;
    [SerializeField] protected EnemyAnimate enemyAnimate;

    protected Transform player;
    protected NavMeshAgent navMeshAgent;
    protected Coroutine afterDeadCoroutine;
    protected float currentHealth;

    protected bool isAttacking;
    protected bool isInWindup;
    protected bool isTakingDamage;
    protected bool isAlive;
    protected bool isMove;
    protected bool isGameEnded;

    protected float lastAttackTime;

    public static Action<bool> onGotDamage;
    public static Action<float> onPlayerDamaged;
    public static Action onEnemyDied;
    public EnemySettings Settings { get => settings; }

    public virtual void Initialize(Transform playerTransform)
    {
        player = playerTransform;
        isGameEnded = false;

        if (navMeshAgent == null)
            navMeshAgent = GetComponent<NavMeshAgent>();

        SetupNavMeshAgent();
        EnableEnemy();
        enemyAnimate.AnimationInit();

        SubscribeAnimations();
    }

    public virtual void Respawn()
    {
        isAttacking = false;
        isInWindup = false;
        isTakingDamage = false;
        isMove = false;

        EnableEnemy();
        enemyAnimate.AnimationInit();
    }

    protected virtual void SubscribeAnimations()
    {
        enemyAnimate.isAttacked += OnAttackAnimationHit;
        enemyAnimate.isAttackEnded += OnAttackAnimationEnd;
        enemyAnimate.isTakedDmg += OnDamageAnimationEnd;
    }

    public virtual void EnableEnemy()
    {
        currentHealth = settings.startHealth;
        enemyCanvas.Init();
        gameObject.SetActive(true);
        isAlive = true;

        if (TryGetComponent(out Collider mainCollider))
            mainCollider.isTrigger = false;
    }

    protected virtual void SetupNavMeshAgent()
    {
        navMeshAgent.speed = settings.moveSpeed;
        navMeshAgent.stoppingDistance = settings.stoppingDistance;
        navMeshAgent.acceleration = settings.acceleration;
        navMeshAgent.angularSpeed = settings.angularSpeed;
        navMeshAgent.autoBraking = true;
        navMeshAgent.isStopped = false;
    }

    protected virtual void Update()
    {
        IsMoving();
        HandleMovement();
        HandleRotationCanvas();
    }

    public virtual void GameEnd()
    {
        isGameEnded = true;
        isMove = false;
        InterruptAttack();
        enemyAnimate.DisableAnim(EnemyAnim.Run);
        if (gameObject.activeSelf) navMeshAgent.isStopped = true;
    }

    protected abstract void HandleMovement();

    protected virtual void IsMoving()
    {
        if (navMeshAgent.isActiveAndEnabled && navMeshAgent.isOnNavMesh 
            && isAlive && !isInWindup && !isAttacking && !isTakingDamage && !isGameEnded) 
        {
            isMove = true;
            if (gameObject.activeSelf) navMeshAgent.isStopped = false;
        } 
        else 
        {
            isMove = false;
            if (gameObject.activeSelf) navMeshAgent.isStopped = true;
        }
    }

    protected virtual void HandleRotationCanvas()
    {
        if (player != null && isAlive)
            enemyCanvas.RotateCanvas(player);
    }

    #region Damage

    public virtual void TakeDamage(float damage, Collider hitTrigger = null)
    {
        if (!isAlive) return;

        float multiplier = GetDamageMultiplier(hitTrigger);
        float finalDamage = damage * multiplier;

        currentHealth -= finalDamage;
        enemyCanvas.ChangeHealth(currentHealth, settings.startHealth);

        if (currentHealth <= 0)
            Die();

        bool isHead = IsHeadshot(hitTrigger);
        onGotDamage?.Invoke(isHead);
        if (isHead)
        {
            InterruptAttack();
            PlayDamageAnimation();
        }
    }

    protected virtual void PlayDamageAnimation()
    {
        if (isTakingDamage || !isAlive) return;
        isTakingDamage = true;
        enemyAnimate.EnableAnim(EnemyAnim.Damage);
    }

    protected virtual void OnDamageAnimationEnd()
    {
        isTakingDamage = false;
    }

    protected virtual void Die()
    {
        isAlive = false;
        enemyAnimate.EnableAnim(EnemyAnim.Die);
        enemyCanvas.gameObject.SetActive(false);

        onEnemyDied?.Invoke();

        if (afterDeadCoroutine == null)
            afterDeadCoroutine = StartCoroutine(AfterDeadWaiting());

        if (TryGetComponent(out Collider mainCollider))
            mainCollider.isTrigger = true;
    }


    protected virtual IEnumerator AfterDeadWaiting()
    {
        yield return new WaitForSeconds(settings.dieDisabledtime);
        gameObject.SetActive(false);
        afterDeadCoroutine = null;
    }

    protected virtual float GetDamageMultiplier(Collider hitTrigger)
    {
        if (hitTrigger == null) return settings.bodyshotMultiplier;
        if (IsHeadshot(hitTrigger)) return settings.headshotMultiplier;
        if (IsLimb(hitTrigger)) return settings.limbMultiplier;
        return settings.bodyshotMultiplier;
    }

    protected abstract bool IsHeadshot(Collider hitTrigger);
    protected abstract bool IsLimb(Collider hitTrigger);

    #endregion

    #region Attack

    protected abstract void TryAttack();
    protected abstract void PerformAttack();

    protected virtual void OnAttackAnimationHit()
    {
        PerformAttack();
    }

    protected virtual void OnAttackAnimationEnd()
    {
        lastAttackTime = Time.time;
        isAttacking = false;
    }

    public virtual void InterruptAttack()
    {
        isAttacking = false;
        isInWindup = false; 
    }

    #endregion

    protected virtual void OnDestroy()
    {
        enemyAnimate.isAttacked -= OnAttackAnimationHit;
        enemyAnimate.isAttackEnded -= OnAttackAnimationEnd;
        enemyAnimate.isTakedDmg -= OnDamageAnimationEnd;
    }
}
