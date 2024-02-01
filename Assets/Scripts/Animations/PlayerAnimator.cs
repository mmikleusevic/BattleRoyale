using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private static string IS_WALKING = "IsWalking";
    private static string IS_DEAD = "IsDead";

    [SerializeField] private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void WalkingAnimation()
    {
        if (animator == null) return;

        Animate(IS_WALKING, true);
    }

    public void DieAnimation()
    {
        if (animator == null) return;

        Animate(IS_DEAD, true);
    }

    private void Animate(string name, bool value)
    {
        animator.SetBool(name, value);
    }
}
