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
    [SerializeField] private List<Vector2> spawnCardPositionList; 

    private List<CardSO> CardsToShow;
    private CardFilter CardFilter;

    private void Awake()
    {
        Hide();

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
    }

    private void OnEnable()
    {
        UpdateVisual();
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

        foreach (CardSO cardSO in CardListSO)
        {
            Transform cardTransform = Instantiate(CardTemplate, CardContainer);
            cardTransform.gameObject.SetActive(true);
            cardTransform.GetComponent<Image>().sprite = cardSO.Sprite;
        }
    }
}
