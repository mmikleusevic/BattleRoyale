using System.Collections;
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

    public IEnumerator SwapTileAnimation()
    {
        yield return StartCoroutine(WaitUntilFinishedAnimation(SWAP));
    }

    public IEnumerator SwapBackTileAnimation()
    {
        yield return StartCoroutine(WaitUntilFinishedAnimation(SWAP_BACK));
    }

    private void Animate(string name)
    {
        animator.SetTrigger(name);
    }

    private IEnumerator WaitUntilFinishedAnimation(string name)
    {
        animator.SetTrigger(name);

        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
        {
            yield return null;
        }
    }
}
