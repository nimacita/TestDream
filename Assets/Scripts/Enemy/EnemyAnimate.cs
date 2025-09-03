using System;
using UnityEngine;
using Utilities.Enums;

public class EnemyAnimate : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Animator animator;
    [SerializeField] private float moveAnimSpeed = 1f;
    [SerializeField] private string moveAnimMultiplier;

    [Header("Bools")]
    [SerializeField] private string runBool;

    [Header("Triggers")]
    [SerializeField] private string attackTrigger;
    [SerializeField] private string damageTrigger;
    [SerializeField] private string dieTrigger;

    public Action isAttacked;
    public Action isAttackEnded;
    public Action isTakedDmg;

    private bool lockedOnDeath;   // <<< добавили

    public void AnimationInit()
    {
        animator.SetFloat(moveAnimMultiplier, moveAnimSpeed);
        lockedOnDeath = false;
        animator.ResetTrigger(attackTrigger);
        animator.ResetTrigger(damageTrigger);
        animator.ResetTrigger(dieTrigger);
        animator.SetBool(runBool, false);
    }

    public void SetRun(bool value)
    {
        if (lockedOnDeath) return;
        if (animator.GetBool(runBool) != value)
            animator.SetBool(runBool, value);
    }

    public void EnableAnim(EnemyAnim enemyAnim)
    {
        if (lockedOnDeath) return;

        switch (enemyAnim)
        {
            case EnemyAnim.Run:
                SetRun(true);
                break;
            case EnemyAnim.Damage:
                animator.SetTrigger(damageTrigger);
                break;
            case EnemyAnim.Die:
                animator.SetTrigger(dieTrigger);
                break;
            case EnemyAnim.Attack:
                animator.SetTrigger(attackTrigger);
                break;
        }
    }

    public void DisableAnim(EnemyAnim enemyAnim)
    {
        if (lockedOnDeath) return;

        switch (enemyAnim)
        {
            case EnemyAnim.Run:
                SetRun(false);
                break;
            case EnemyAnim.Damage:
                animator.ResetTrigger(damageTrigger);
                break;
            case EnemyAnim.Die:
                animator.ResetTrigger(dieTrigger);
                break;
            case EnemyAnim.Attack:
                animator.ResetTrigger(attackTrigger);
                break;
        }
            
    }

    // Вызвать при смерти, чтобы никакие другие анимации не «перебили» смерть
    public void PlayDieAndLock()
    {
        if (lockedOnDeath) return;
        SetRun(false);
        animator.ResetTrigger(attackTrigger);
        animator.ResetTrigger(damageTrigger);
        animator.SetTrigger(dieTrigger);
        lockedOnDeath = true;
    }

    public void AttackInvoke()
    {
        isAttacked?.Invoke();
    }

    public void AttackEndInvoke()
    {
        isAttackEnded?.Invoke();
    }

    public void TakedDamageInvoke()
    {
        isTakedDmg?.Invoke();
    }


}
