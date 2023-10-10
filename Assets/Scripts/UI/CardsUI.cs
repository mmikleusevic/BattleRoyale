using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardsUI : MonoBehaviour
{
    public static CardsUI Instance { get; private set; }

    [SerializeField] private Button CloseButton;
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

        CardFilter = new CardFilter();

        CardTypeDropdown.onValueChanged.AddListener((int val) =>
        {
            CardFilter.CardType = (CardType)val;
            GetCardsForUI();
        });

        CardsToShow = CardListSO;
        UpdateVisual();
    }

    private void Start()
    {
        Hide();
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    public void GetCardsForUI()
    {
        CardsToShow = CardFilter.GetFilteredCards(CardListSO);

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
            cardTransform.GetComponent<Image>().sprite = CardsToShow[i].Sprite;
        }
    }
}
