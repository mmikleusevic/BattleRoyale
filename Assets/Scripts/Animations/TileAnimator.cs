using UnityEngine;

public class TileAnimator : MonoBehaviour
{
    private static readonly string IS_CLOSED = "IsClosed";

    [SerializeField] private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void CloseTileAnimation()
    {
        Animate(IS_CLOSED);
    }

    private void Animate(string name)
    {
        animator.SetTrigger(name);
    }
}
