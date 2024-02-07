using UnityEngine;

public class CardAnimator : MonoBehaviour
{
    private static string IS_CLOSED = "IsClosed";

    [SerializeField] private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void CloseCardAnimation()
    {
        Animate(IS_CLOSED, true);
    }

    private void Animate(string name, bool value)
    {
        animator.SetBool(name, value);
    }
}
