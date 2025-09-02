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

    public void AnimationInit()
    {
        animator.SetFloat(moveAnimMultiplier, moveAnimSpeed);
    }

    public void EnableAnim(EnemyAnim enemyAnim)
    {
        switch (enemyAnim)
        {
            case EnemyAnim.Run:
                if(!animator.GetBool(runBool))
                    animator.SetBool(runBool, true);
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
        switch (enemyAnim)
        {
            case EnemyAnim.Run:
                if (animator.GetBool(runBool))
                    animator.SetBool(runBool, false);
                break;
        }
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
