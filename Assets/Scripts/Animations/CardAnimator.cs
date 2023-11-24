using UnityEngine;

public class CardAnimator : MonoBehaviour
{
    private static string IS_OPEN = "IsOpen";

    [SerializeField] private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void OpenCard()
    {
        if (animator == null) return;

        animator.SetBool(IS_OPEN, true);
    }
}
