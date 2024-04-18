using UnityEngine;

public class TileAnimator : MonoBehaviour
{
    private static readonly string IS_CLOSED = "IsClosed";
    private static readonly string SWAP = "Swap";
    private static readonly string SWAP_BACK = "SwapBack";

    [SerializeField] private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void CloseTileAnimation()
    {
        Animate(IS_CLOSED);
    }

    public void SwapTileAnimation()
    {
        Animate(SWAP);
    }

    public void SwapBackTileAnimation()
    {
        Animate(SWAP_BACK);
    }

    private void Animate(string name)
    {
        animator.SetTrigger(name);
    }
}
