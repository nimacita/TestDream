using UnityEngine;
using UnityEngine.AI;
using Utilities.Enums;

public class EnemyBase : MonoBehaviour
{

    [Header("Enemy Settings")]
    [SerializeField] private EnemySettings settings;

    [Header("Attack Settings")]
    [SerializeField] private LayerMask playerLayer;

    [Header("Health Settings")]
    private float _currHealth;

    [Header("Attack Detection")]
    [SerializeField] private Vector3 attackOffset = new Vector3(0, 1f, 0);
    [SerializeField] private float attackRadius = 1f;

    [Header("Hit Triggers")]
    [SerializeField] private Collider headCollider;
    [SerializeField] private Collider bodyCollider;
    [SerializeField] private Collider[] limbColliders;
    [SerializeField] private Collider mainCollider;

    [Header("Components")]
    [SerializeField] private EnemyCanvas enemyCanvas;
    [SerializeField] private EnemyAnimate enemyAnimate;

    private Transform player;
    private NavMeshAgent navMeshAgent;
    private float lastAttackTime;
    private bool isAttacking;
    private bool isInWindup;
    private bool isTakedDamage;
    private bool isAlive;
    private float windupStartTime;

    public void Initialize(Transform playerTransform)
    {
        player = playerTransform;
        if(navMeshAgent == null) navMeshAgent = GetComponent<NavMeshAgent>();
        SetupNavMeshAgent();
        EnableEnemy();
        enemyAnimate.AnimationInit();

        enemyAnimate.isAttacked += ExecuteAttack;
        enemyAnimate.isTakedDmg += TakedDamageEndAnim;
        enemyAnimate.isAttackEnded += FinishAttack;
    }

    public void EnableEnemy()
    {
        _currHealth = settings.startHealth;
        enemyCanvas.Init();
        gameObject.SetActive(true);
        mainCollider.isTrigger = false;
        isAlive = true;
    }

    private void SetupNavMeshAgent()
    {
        navMeshAgent.speed = settings.moveSpeed;
        navMeshAgent.stoppingDistance = settings.stoppingDistance;
        navMeshAgent.acceleration = settings.acceleration;
        navMeshAgent.angularSpeed = settings.angularSpeed;
        navMeshAgent.autoBraking = true;

        navMeshAgent.isStopped = false;
    }

    private void Update()
    {
        IsMoving();
        EnemyMove();
        RotateCanvas();
    }

    #region Enemy Move

    private void EnemyMove()
    {
        if (player == null || !isAlive) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= settings.attackRange && !isAttacking && !isInWindup)
        {
            TryStartAttack();
        }
        else if (distanceToPlayer > settings.attackRange && !isAttacking && !isInWindup)
        {
            ChasePlayer();
        }

        if (navMeshAgent.isStopped)
        {
            enemyAnimate.DisableAnim(EnemyAnim.Run);
        }
        else
        {
            enemyAnimate.EnableAnim(EnemyAnim.Run);
        }
    }

    private void ChasePlayer()
    {
        if (navMeshAgent.isActiveAndEnabled && navMeshAgent.isOnNavMesh && !isAttacking && !isInWindup)
        {
            navMeshAgent.SetDestination(player.position);
        }
    }

    private void IsMoving()
    {
        if (navMeshAgent.isActiveAndEnabled && navMeshAgent.isOnNavMesh
            && isAlive && !isInWindup && !isAttacking && !isTakedDamage)
        {
            navMeshAgent.isStopped = false;
        }
        else
        {
            navMeshAgent.isStopped = true;
        }
    }
    #endregion

    #region Attack
    private void TryStartAttack()
    {
        if (Time.time - lastAttackTime >= settings.attackCooldown 
            && !isInWindup && !isAttacking)
        {
            StartAttackWindup();
        }
    }

    private void StartAttackWindup()
    {
        isInWindup = true;
        windupStartTime = Time.time;

        // Поворачиваемся к игроку во время замаха
        if (player != null)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            directionToPlayer.y = 0; // Сохраняем только горизонтальное вращение
            if (directionToPlayer != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(directionToPlayer);
            }
        }

        StartAttack();
    }

    private void StartAttack()
    {
        isInWindup = false;
        isAttacking = true;

        //включаем анимацию атки
        enemyAnimate.EnableAnim(EnemyAnim.Attack);
    }

   //наносим урон атакой
    private void ExecuteAttack()
    {
        // Выполняем обнаружение цели
        PerformAttackDetection();
    }

    private void PerformAttackDetection()
    {
        Vector3 attackPosition = transform.position + transform.TransformDirection(attackOffset);

        Collider[] hitColliders = Physics.OverlapSphere(attackPosition, attackRadius, playerLayer);

        foreach (Collider collider in hitColliders)
        {
            if (collider.CompareTag("Player"))
            {
                DamagePlayer(collider.gameObject);
                break;
            }
        }


        // Визуализация области атаки (только в редакторе)
#if UNITY_EDITOR
        DebugDrawAttackArea(attackPosition);
#endif
    }

    private void DamagePlayer(GameObject playerObject)
    {
        Debug.Log("Player Damage");
    }

    //заканчиваем атаку
    private void FinishAttack()
    {
        lastAttackTime = Time.time;
        isAttacking = false;
    }

    public void InterruptAttack()
    {
        if (isInWindup || isAttacking)
        {
            isInWindup = false;
            isAttacking = false;
        }
    }
    #endregion

    #region Take Damage

    private void RotateCanvas()
    {
        if (player == null || !isAlive) return;
        enemyCanvas.RotateCanvas(player);
    }

    public void TakeDamage(float baseDamage, Collider hitTrigger = null)
    {
        if (!isAlive) return;

        float damageMultiplier = GetDamageMultiplier(hitTrigger);
        float finalDamage = baseDamage * damageMultiplier;

        if (hitTrigger == headCollider)
        {
            InterruptAttack();
            TakedDamageStartAnim();
        }

        _currHealth -= finalDamage;
        enemyCanvas.ChangeHealth(_currHealth, settings.startHealth);
        if (_currHealth <= 0) EnemyDead();
    }

    private void TakedDamageStartAnim()
    {
        if (isTakedDamage) return;
        isTakedDamage = true;
        enemyAnimate.EnableAnim(EnemyAnim.Damage);
    }

    private void TakedDamageEndAnim()
    {
        if (!isTakedDamage) return;
        isTakedDamage = false;
    }

    private void EnemyDead()
    {
        isAlive = false;
        enemyAnimate.EnableAnim(EnemyAnim.Die);
        enemyCanvas.gameObject.SetActive(false);
        mainCollider.isTrigger = true;
    }

    private float GetDamageMultiplier(Collider hitTrigger)
    {
        if (hitTrigger == null) return settings.bodyshotMultiplier;

        if (hitTrigger == headCollider)
        {
            return settings.headshotMultiplier;
        }
        else if (hitTrigger == bodyCollider)
        {
            return settings.bodyshotMultiplier;
        }
        else if (IsLimbCollider(hitTrigger))
        {
            return settings.limbMultiplier;
        }

        return settings.bodyshotMultiplier;
    }

    private bool IsLimbCollider(Collider collider)
    {
        if (limbColliders != null && limbColliders.Length > 0)
        {
            foreach (Collider limbCol in limbColliders)
            {
                if (limbCol == collider) return true;
            }
        }

        return false;
    }
    #endregion

#if UNITY_EDITOR
    private void DebugDrawAttackArea(Vector3 attackPosition)
    {
        Debug.DrawRay(attackPosition, Vector3.up * 0.5f, Color.red, 1f);
        Debug.DrawRay(attackPosition, -Vector3.up * 0.5f, Color.red, 1f);
        Debug.DrawRay(attackPosition, Vector3.forward * attackRadius, Color.red, 1f);
        Debug.DrawRay(attackPosition, -Vector3.forward * attackRadius, Color.red, 1f);
        Debug.DrawRay(attackPosition, Vector3.right * attackRadius, Color.red, 1f);
        Debug.DrawRay(attackPosition, -Vector3.right * attackRadius, Color.red, 1f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 attackPosition = transform.position + transform.TransformDirection(attackOffset);
        Gizmos.DrawWireSphere(attackPosition, attackRadius);

        // Отображаем радиус атаки
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, settings.attackRange);

        // Отображаем зону замаха (если в процессе)
        if (Application.isPlaying && isInWindup)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, 1f);
        }
    }
#endif

    private void OnDestroy()
    {
        enemyAnimate.isAttacked -= ExecuteAttack;
        enemyAnimate.isTakedDmg -= TakedDamageEndAnim;
        enemyAnimate.isAttackEnded -= FinishAttack;
    }
}
