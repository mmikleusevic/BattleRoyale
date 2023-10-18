using UnityEngine;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Color BaseColor;
    [SerializeField] private Color OffsetColor;
    [SerializeField] private SpriteRenderer Renderer;
    [SerializeField] private GameObject Highlight;

    public void ColorTile(bool isOffset)
    {
        Renderer.color = isOffset ? OffsetColor : BaseColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Show();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Hide();
    }

    private void Show()
    {
        Highlight.SetActive(true);
    }

    private void Hide()
    {
        Highlight.SetActive(false);
    }
}
