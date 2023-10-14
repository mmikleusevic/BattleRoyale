using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SingleCardUI : MonoBehaviour, IPointerClickHandler
{
    public static event EventHandler OnCardImageClick;

    private static bool Zoomed = false;
    public Sprite Sprite { get; private set; }

    private void OnEnable()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.localScale = Vector3.zero;

        LeanTween.scale(GetComponent<RectTransform>(), Vector3.one, 0.4f).setDelay(0.3f);
    }

    public void Destroy()
    {
        LeanTween.scale(GetComponent<RectTransform>(), Vector3.zero, 0.3f).setDestroyOnComplete(true);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnCardImageClick?.Invoke(this, EventArgs.Empty);       
    }

    public void ToggleZoom(Image zoomedSingleCardBackground, Transform zoomedSingleCardTemplate, Transform zoomedSingleCardContainer)
    {
        if (!Zoomed)
        {
            zoomedSingleCardBackground.gameObject.SetActive(true);

            Transform cardTransform = Instantiate(zoomedSingleCardTemplate, zoomedSingleCardContainer);

            cardTransform.gameObject.SetActive(true);

            SetSprite(cardTransform, Sprite);
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

        Zoomed = !Zoomed;
    }

    public void SetSprite(Transform cardTransform, Sprite sprite)
    {
        if (!Sprite)
        {
            cardTransform.GetComponentInChildren<Image>().sprite = sprite;
            Sprite = sprite;

            return;
        }

        cardTransform.GetComponentInChildren<Image>().sprite = Sprite;
    }
}
