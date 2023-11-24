using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private static string IS_WALKING = "IsWalking";

    [SerializeField] private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void OpenCard()
    {
        if (animator == null) return;

        animator.SetBool(IS_WALKING, true);
    }
}
