using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SingleCardUI : MonoBehaviour, IPointerDownHandler
{
    public static event EventHandler OnCardImageClick;

    private RectTransform cardRectTransform;

    private static bool zoomed = false;
    public Sprite sprite { get; private set; }

    private void Awake()
    {
        cardRectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.localScale = Vector3.zero;
        cardRectTransform.DOScale(Vector3.one, 0.4f).SetDelay(0.3f);
    }

    public void Destroy()
    {
        cardRectTransform.DOScale(Vector3.zero, 0.3f).OnComplete(() => Destroy(gameObject)).Kill(true);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnCardImageClick?.Invoke(this, EventArgs.Empty);
    }

    public void ToggleZoom(Image zoomedSingleCardBackground, Transform zoomedSingleCardTemplate, Transform zoomedSingleCardContainer)
    {
        if (!zoomed)
        {
            zoomedSingleCardBackground.gameObject.SetActive(true);

            Transform cardTransform = Instantiate(zoomedSingleCardTemplate, zoomedSingleCardContainer);

            cardTransform.gameObject.SetActive(true);

            SetSprite(cardTransform, sprite);
        }
        else
        {
            foreach (Transform child in zoomedSingleCardContainer)
            {
                if (child == zoomedSingleCardTemplate) continue;
                Destroy();
            }

            zoomedSingleCardBackground.gameObject.SetActive(false);
        }

        zoomed = !zoomed;
    }

    public void SetSprite(Transform cardTransform, Sprite sprite)
    {
        if (!this.sprite)
        {
            cardTransform.GetComponentInChildren<Image>().sprite = sprite;
            this.sprite = sprite;

            return;
        }

        cardTransform.GetComponentInChildren<Image>().sprite = this.sprite;
    }
}
