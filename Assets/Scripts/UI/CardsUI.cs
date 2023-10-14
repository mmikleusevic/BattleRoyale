using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardsUI : MonoBehaviour
{
    public static CardsUI Instance { get; private set; }

    [SerializeField] private Button CloseButton;
    [SerializeField] private Button PageLeftButton;
    [SerializeField] private Button PageRightButton;
    [SerializeField] private List<Card> CardList;
    [SerializeField] private TMP_Dropdown CardTypeDropdown;
    [SerializeField] private Transform CardTemplateContainer;
    [SerializeField] private Transform CardTemplate;
    [SerializeField] private Image ZoomedSingleCardBackground;
    [SerializeField] private Transform ZoomedSingleCardContainer;
    [SerializeField] private Transform ZoomedSingleCardTemplate;

    private PagedList<Card> PagedCardList;

    private int PageSize = 8;
    private int FirstPage = 1;
    private int Page = 1;

    private void Awake()
    {
        Instance = this;

        CloseButton.onClick.AddListener(() =>
        {
            Hide();
        });

        PageLeftButton.onClick.AddListener(() =>
        {
            PageLeft();
        });

        PageRightButton.onClick.AddListener(() =>
        {
            PageRight();
        });

        CardTypeDropdown.onValueChanged.AddListener((int val) =>
        {
            OnCardTypeChanged((CardType)val);
        });

        SingleCardUI.OnCardImageClick += SingleCardUI_OnCardImageClick;

        ZoomedSingleCardBackground.gameObject.SetActive(false);
    }

    private void Start()
    {
        Hide();
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
        singleCardUI.ToggleZoom(ZoomedSingleCardBackground, ZoomedSingleCardTemplate, ZoomedSingleCardContainer);
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
        foreach (Transform child in CardTemplateContainer)
        {
            if (child == CardTemplate) continue;
            child.GetComponent<SingleCardUI>().Destroy();
        }
    }

    private void SetObject()
    {
        foreach (Card card in PagedCardList.Items)
        {
            Transform cardTransform = Instantiate(CardTemplate, CardTemplateContainer);

            cardTransform.gameObject.SetActive(true);

            Sprite sprite = card.Sprite;

            cardTransform.GetComponent<SingleCardUI>().SetSprite(cardTransform, sprite);
        }
    }

    private void PageLeft()
    {
        if (PagedCardList.HasPreviousPage)
        {
            Page--;

            GetCardsForUI();

            if (!PagedCardList.HasPreviousPage) PageLeftButton.gameObject.SetActive(false);
            if (PagedCardList.HasNextPage) PageRightButton.gameObject.SetActive(true);
        }
    }

    private void PageRight()
    {
        if (PagedCardList.HasNextPage)
        {
            Page++;

            GetCardsForUI();

            if (!PagedCardList.HasNextPage) PageRightButton.gameObject.SetActive(false);
            if (PagedCardList.HasPreviousPage) PageLeftButton.gameObject.SetActive(true);
        }
    }

    private void OnCardTypeChanged(CardType cardType)
    {
        CardFilter.CardType = cardType;
        Page = FirstPage;
        PageLeftButton.gameObject.SetActive(false);

        GetCardsForUI();

        if (PagedCardList.HasNextPage)
        {
            PageRightButton.gameObject.SetActive(true);
        }
        else
        {
            PageRightButton.gameObject.SetActive(false);
        }
    }

    private void GetPagedCardList()
    {
        PagedCardList = CardFilter.GetFilteredCards(CardList, Page, PageSize);
    }
}
