using UnityEngine;
using UnityEngine.UI;

public class CardGridLayoutGroupResize : MonoBehaviour
{
    [SerializeField] private GameObject container;
    [SerializeField] private RectTransform containerRectTransform;
    [SerializeField] private GridLayoutGroup containerGridLayoutGroup;

    Vector2 lastContainerSize = Vector2.zero;

    private void Update()
    {
        Vector2 containerSize = new Vector2((int)containerRectTransform.rect.size.x, (int)containerRectTransform.rect.size.y);

        if (lastContainerSize == containerSize) return;

        lastContainerSize.x = containerSize.x;
        lastContainerSize.y = containerSize.y;

        Vector2 newSize = new Vector2(containerSize.x / 4, containerSize.y / 2);
        containerGridLayoutGroup.cellSize = newSize;
    }
}
