using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private static readonly string IS_MOVING = "IsMoving";
    private static readonly string IS_DEAD = "IsDead";

    [SerializeField] private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void MoveAnimation()
    {
        Animate(IS_MOVING, true);
    }

    public void StopMovingAnimation()
    {
        Animate(IS_MOVING, false);
    }

    public void DieAnimation()
    {
        Animate(IS_DEAD, true);
    }

    public void AliveAnimation()
    {
        Animate(IS_DEAD, false);
    }

    private void Animate(string name, bool value)
    {
        animator.SetBool(name, value);
    }
}
