using UnityEngine;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Color baseColor;
    [SerializeField] private Color offsetColor;
    [SerializeField] private SpriteRenderer Renderer;
    [SerializeField] private GameObject highlight;

    public void ColorTile(bool isOffset)
    {
        Renderer.color = isOffset ? offsetColor : baseColor;
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
        highlight.SetActive(true);
    }

    private void Hide()
    {
        highlight.SetActive(false);
    }
}
