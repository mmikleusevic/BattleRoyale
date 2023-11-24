using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class Card : NetworkBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private GameObject highlight;

    private bool isOpen = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        Show();

        if (!isOpen)
        {
            OpenCard();

            isOpen = true;
        }
    }

    private void OpenCard()
    {
        GetComponent<CardAnimator>().OpenCard();
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
