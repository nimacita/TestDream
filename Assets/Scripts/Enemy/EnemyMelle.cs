using UnityEngine;
using Utilities.Enums;

public class EnemyMelle : EnemyBase
{
    [Header("Attack Detection")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private Vector3 attackOffset = new Vector3(0, 1f, 0);
    [SerializeField] private float attackRadius = 1f;

    [Header("Hit Colliders")]
    [SerializeField] private Collider headCollider;
    [SerializeField] private Collider bodyCollider;
    [SerializeField] private Collider[] limbColliders;

    private bool _isClosedPlayer = false;

    protected override void HandleMovement()
    {
        if (isGameEnded) return;
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= settings.attackRange && !isAttacking && !isInWindup)
        {
            TryAttack();
        }
        else
        {
            _isClosedPlayer = (distance < settings.attackRange);
            if (isMove)
            {
                navMeshAgent.SetDestination(player.position);
                enemyAnimate.EnableAnim(EnemyAnim.Run);
            }
            else
            {
                enemyAnimate.DisableAnim(EnemyAnim.Run);
            }
        }
    }

    protected override void IsMoving()
    {
        if (navMeshAgent.isActiveAndEnabled && navMeshAgent.isOnNavMesh
            && isAlive && !isInWindup && !isAttacking && !isTakingDamage && !_isClosedPlayer && !isGameEnded)
        {
            isMove = true;
            navMeshAgent.isStopped = false;
        }
        else
        {
            isMove = false;
            navMeshAgent.isStopped = true;
        }
    }

    protected override void TryAttack()
    {
        if (Time.time - lastAttackTime >= settings.attackCooldown)
        {
            isAttacking = true;
            isInWindup = false;

            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0;
            transform.rotation = Quaternion.LookRotation(direction);

            enemyAnimate.EnableAnim(EnemyAnim.Attack);
        }
    }

    protected override void PerformAttack()
    {
        Vector3 attackPosition = transform.position + transform.TransformDirection(attackOffset);
        Collider[] hitColliders = Physics.OverlapSphere(attackPosition, attackRadius, playerLayer);

        foreach (Collider col in hitColliders)
        {
            if (col.CompareTag("Player"))
            {
                onPlayerDamaged?.Invoke(settings.attackDamage);
                break;
            }
        }
    }

    protected override bool IsHeadshot(Collider hitTrigger)
    {
        return hitTrigger == headCollider;
    }

    protected override bool IsLimb(Collider hitTrigger)
    {
        if (limbColliders == null) return false;
        foreach (var limb in limbColliders)
        {
            if (limb == hitTrigger) return true;
        }
        return false;
    }

#if UNITY_EDITOR

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
}
