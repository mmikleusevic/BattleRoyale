using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardsUI : MonoBehaviour
{
    public static CardsUI Instance { get; private set; }

    [SerializeField] private Button CloseButton;
    [SerializeField] private Button PageLeftButton;
    [SerializeField] private Button PageRightButton;
    [SerializeField] private List<CardSO> CardListSO;
    [SerializeField] private TMP_Dropdown CardTypeDropdown;
    [SerializeField] private Transform CardContainer;
    [SerializeField] private Transform CardTemplate;

    private List<CardSO> CardsToShow;
    private CardFilter CardFilter;

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

        CardFilter = new CardFilter();

        CardTypeDropdown.onValueChanged.AddListener((int val) =>
        {
            OnCardTypeChanged((CardType)val);
        });

        CardsToShow = new List<CardSO>();
    }

    private void Start()
    {
        Hide();
    }

    public void Show()
    {
        gameObject.SetActive(true);
        GetCardsForUI();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    public void GetCardsForUI()
    {
        CardsToShow = CardFilter.GetFilteredCards(CardListSO);

        if (Enumerable.SequenceEqual(CardsToShow, CardListSO)) return;

        UpdateVisual();
    }

    private void UpdateVisual()
    {
        foreach (Transform child in CardContainer)
        {
            if (child == CardTemplate) continue;
            Destroy(child.gameObject);
        }

        SetObject();
    }

    private void SetObject()
    {
        for (int i = 0; i < CardsToShow.Count; i++)
        {
            Transform cardTransform = Instantiate(CardTemplate, CardContainer);

            cardTransform.gameObject.SetActive(true);

            Sprite sprite = CardsToShow[i].Sprite;
            cardTransform.GetComponentInChildren<Image>().sprite = sprite;
        }
    }

    private void PageLeft()
    {
        if (CardFilter.Page > 0)
        {
            CardFilter.Page--;

            GetCardsForUI();

            if (CardFilter.Page == 0) PageLeftButton.gameObject.SetActive(false);
            if (CardFilter.Page < CardFilter.MaxPage) PageRightButton.gameObject.SetActive(true);
        }
    }

    private void PageRight()
    {
        if (CardFilter.Page < CardFilter.MaxPage)
        {
            CardFilter.Page++;

            GetCardsForUI();

            if (CardFilter.Page == CardFilter.MaxPage) PageRightButton.gameObject.SetActive(false);
            if (CardFilter.Page > 0) PageLeftButton.gameObject.SetActive(true);

        }
    }

    private void OnCardTypeChanged(CardType cardType)
    {
        CardFilter.CardType = cardType;
        CardFilter.Page = 0;
        PageLeftButton.gameObject.SetActive(false);

        GetCardsForUI();

        if (CardFilter.MaxPage > 0)
        {
            PageRightButton.gameObject.SetActive(true);
        }
        else
        {
            PageRightButton.gameObject.SetActive(false);
        }
    }
}
