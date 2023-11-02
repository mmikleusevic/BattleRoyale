using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardsUI : MonoBehaviour
{
    public static CardsUI Instance { get; private set; }

    [SerializeField] private Button closeButton;
    [SerializeField] private Button pageLeftButton;
    [SerializeField] private Button pageRightButton;
    [SerializeField] private List<CardSO> cardList;
    [SerializeField] private TMP_Dropdown cardTypeDropdown;
    [SerializeField] private Transform cardTemplateContainer;
    [SerializeField] private Transform cardTemplate;
    [SerializeField] private Image zoomedSingleCardBackground;
    [SerializeField] private Transform zoomedSingleCardContainer;
    [SerializeField] private Transform zoomedSingleCardTemplate;

    private PagedList<CardSO> pagedCardList;

    private int pageSize = 8;
    private int firstPage = 1;
    private int page = 1;

    private void Awake()
    {
        Instance = this;

        closeButton.onClick.AddListener(() =>
        {
            Hide();
        });

        pageLeftButton.onClick.AddListener(() =>
        {
            PageLeft();
        });

        pageRightButton.onClick.AddListener(() =>
        {
            PageRight();
        });

        cardTypeDropdown.onValueChanged.AddListener((int val) =>
        {
            OnCardTypeChanged((CardType)val);
        });

        zoomedSingleCardBackground.gameObject.SetActive(false);
    }

    private void Start()
    {
        Hide();
    }

    private void OnEnable()
    {
        SingleCardUI.OnCardImageClick += SingleCardUI_OnCardImageClick;
    }

    private void OnDisable()
    {
        SingleCardUI.OnCardImageClick -= SingleCardUI_OnCardImageClick;
    }

    public void Show()
    {
        gameObject.SetActive(true);
        OnCardTypeChanged(CardType.All);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void SingleCardUI_OnCardImageClick(object sender, System.EventArgs e)
    {
        SingleCardUI singleCardUI = sender as SingleCardUI;
        singleCardUI.ToggleZoom(zoomedSingleCardBackground, zoomedSingleCardTemplate, zoomedSingleCardContainer);
    }

    public void GetCardsForUI()
    {
        GetPagedCardList();

        UpdateVisual();
    }

    private void UpdateVisual()
    {
        RemoveOldUICardElements();

        SetObject();
    }

    private void RemoveOldUICardElements()
    {
        foreach (Transform child in cardTemplateContainer)
        {
            if (child == cardTemplate) continue;
            child.GetComponent<SingleCardUI>().Destroy();
        }
    }

    private void SetObject()
    {
        foreach (CardSO card in pagedCardList.items)
        {
            Transform cardTransform = Instantiate(cardTemplate, cardTemplateContainer);

            cardTransform.gameObject.SetActive(true);

            Sprite sprite = card.cardSprite;

            cardTransform.GetComponent<SingleCardUI>().SetSprite(cardTransform, sprite);
        }
    }

    private void PageLeft()
    {
        if (pagedCardList.hasPreviousPage)
        {
            page--;

            GetCardsForUI();

            if (!pagedCardList.hasPreviousPage) pageLeftButton.gameObject.SetActive(false);
            if (pagedCardList.hasNextPage) pageRightButton.gameObject.SetActive(true);
        }
    }

    private void PageRight()
    {
        if (pagedCardList.hasNextPage)
        {
            page++;

            GetCardsForUI();

            if (!pagedCardList.hasNextPage) pageRightButton.gameObject.SetActive(false);
            if (pagedCardList.hasPreviousPage) pageLeftButton.gameObject.SetActive(true);
        }
    }

    private void OnCardTypeChanged(CardType cardType)
    {
        CardFilter.cardType = cardType;
        page = firstPage;
        pageLeftButton.gameObject.SetActive(false);

        GetCardsForUI();

        if (pagedCardList.hasNextPage)
        {
            pageRightButton.gameObject.SetActive(true);
        }
        else
        {
            pageRightButton.gameObject.SetActive(false);
        }
    }

    private void GetPagedCardList()
    {
        pagedCardList = CardFilter.GetFilteredCards(cardList, page, pageSize);
    }
}
