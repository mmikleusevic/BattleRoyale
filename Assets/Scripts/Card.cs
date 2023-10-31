using UnityEngine;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Material frontMaterial;
    [SerializeField] private GameObject highlight;

    public void AssignMaterial(Material newMaterial)
    {
        frontMaterial = newMaterial;
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
