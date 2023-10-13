using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SingleCardUI : MonoBehaviour, IPointerClickHandler
{
    private static Image ZoomedSingleCardBackground;
    private static Transform ZoomedSingleCardContainer;
    private static Transform ZoomedSingleCardTemplate;

    private static bool Zoomed = false;
    public Sprite Sprite { get; private set; }

    private void Awake()
    {
        if (!ZoomedSingleCardBackground) ZoomedSingleCardBackground = CardsUI.Instance.GetZoomedSingleCardBackgroundImage();
        if (!ZoomedSingleCardContainer) ZoomedSingleCardContainer = CardsUI.Instance.GetZoomedSingleCardContainer();
        if (!ZoomedSingleCardTemplate) ZoomedSingleCardTemplate = CardsUI.Instance.GetZoomedSingleCardContainerTemplate();
    }

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
        if (!Zoomed)
        {
            ZoomedSingleCardBackground.gameObject.SetActive(true);

            Transform cardTransform = Instantiate(ZoomedSingleCardTemplate, ZoomedSingleCardContainer);

            cardTransform.gameObject.SetActive(true);

            SetSprite(cardTransform, Sprite);
        }
        else
        {
            foreach (Transform child in ZoomedSingleCardContainer)
            {
                if (child == ZoomedSingleCardTemplate) continue;
                Destroy();
            }

            ZoomedSingleCardBackground.gameObject.SetActive(false);
        }

        Zoomed = !Zoomed;
    }

    public void SetSprite(Transform cardTransform, Sprite sprite)
    {
        cardTransform.GetComponentInChildren<Image>().sprite = sprite;

        if (Sprite) return;

        Sprite = sprite;
    }
}
