using UnityEngine;

public class PlayerAnim : MonoBehaviour
{
    [Header("Bools")]
    [SerializeField] private string run;
    [SerializeField] private string back;
    [SerializeField] private string left;
    [SerializeField] private string right;
    [SerializeField] private string reload;

    [Header("Animator Setting")]
    [SerializeField] private Animator animator;

    public void SetActiveRun(bool value)
    {
        animator.SetBool(run, value);
    }

    public void SetActiveBack(bool value)
    {
        animator.SetBool(back, value);
    }

    public void SetActiveLeft(bool value)
    {
        animator.SetBool(left, value);
        if(value) animator.SetBool(right, false);
    }

    public void SetActiveRight(bool value)
    {
        animator.SetBool(right, value);
        if (value) animator.SetBool(left, false);
    }

    public void SetReload(bool value)
    {
        if (value)
        {
            if (!animator.GetBool(reload))
                animator.SetBool(reload, true);
        }
        else
        {
            if (animator.GetBool(reload))
                animator.SetBool(reload, false);
        }
    }
}
