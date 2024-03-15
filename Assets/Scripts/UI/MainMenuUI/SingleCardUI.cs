using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SingleCardUI : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private Image zoomedCardBackground;
    [SerializeField] private RectTransform zoomedCardTemplateContainer;
    [SerializeField] private RectTransform zoomedCardTemplate;

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
        ToggleZoom();
    }

    public void ToggleZoom()
    {
        if (!zoomed)
        {
            zoomedCardBackground.gameObject.SetActive(true);

            Transform cardTransform = Instantiate(zoomedCardTemplate, zoomedCardTemplateContainer);

            cardTransform.gameObject.SetActive(true);

            SetSprite(cardTransform, sprite);
        }
        else
        {
            foreach (Transform child in zoomedCardTemplateContainer)
            {
                if (child == zoomedCardTemplate) continue;
                Destroy();
            }

            zoomedCardBackground.gameObject.SetActive(false);
        }

        zoomed = !zoomed;
    }


    public void SetSprite(Transform cardTransform, Sprite sprite)
    {
        Image image;

        if (!this.sprite)
        {
            image = cardTransform.GetComponentInChildren<Image>();
            image.preserveAspect = true;
            image.sprite = sprite;

            this.sprite = sprite;

            return;
        }

        image = cardTransform.GetComponentInChildren<Image>();
        image.preserveAspect = true;
        image.sprite = this.sprite;
    }
}
