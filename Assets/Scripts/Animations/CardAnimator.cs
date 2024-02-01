using UnityEngine;

public class CardAnimator : MonoBehaviour
{
    private static string IS_CLOSED = "IsClosed";

    [SerializeField] private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void CloseCard()
    {
        if (animator == null) return;

        Animate(IS_CLOSED, true);
    }

    private void Animate(string name, bool value)
    {
        animator.SetBool(name, value);
    }
}
