using UnityEngine;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Material frontMaterial;
    [SerializeField] private GameObject highlight;

    public void SetupCard(CardSO cardSO)
    {
        GetComponent<Renderer>().material.SetTexture("Cost"+cardSO.cost, cardSO.costSprite.texture);
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
