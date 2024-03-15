using UnityEngine;
using UnityEngine.UI;

public class GridLayoutGroupResize : MonoBehaviour
{
    [SerializeField] private GameObject container;

    private void Update()
    {
        float width = container.GetComponent<RectTransform>().rect.width;
        float height = container.GetComponent<RectTransform>().rect.height;
        Vector2 newSize = new Vector2(width / 4, height / 2);
        container.GetComponent<GridLayoutGroup>().cellSize = newSize;
    }
}
