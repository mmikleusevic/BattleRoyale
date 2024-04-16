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
        Animate(IS_CLOSED, true);
    }

    private void Animate(string name, bool value)
    {
        animator.SetBool(name, value);
    }
}
