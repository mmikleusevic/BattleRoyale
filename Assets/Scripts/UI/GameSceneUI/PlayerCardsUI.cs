using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCardsUI : MonoBehaviour
{
    public static event Action OnPlayerCardsUIClosed;

    [SerializeField] private RectTransform PlayerCardsUIRectTransform;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Transform verticalGridContainer;
    [SerializeField] private Transform container;
    [SerializeField] private Transform template;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button showUneqippedCardsButton;
    [SerializeField] private Button takeCardButton;

    private Player player;

    private void Awake()
    {
        closeButton.onClick.AddListener(async () =>
        {
            if (StateManager.Instance.GetState() == StateEnum.PlayerPreturn)
            {
                await StateManager.Instance.EndState();
            }

            HideWithAnimation();

            OnPlayerCardsUIClosed?.Invoke();
        });

        showUneqippedCardsButton.onClick.AddListener(() =>
        {
            //TODO
        });

        takeCardButton.onClick.AddListener(() =>
        {
            //TODO
        });

        PlayerPreturn.OnPlayerPreturn += PlayerPreturn_OnPlayerPreturn;
        AttackPlayerInfoUI.OnShowPlayerEquippedCards += AttackPlayerInfoUI_OnShowPlayerEquippedCards;

        takeCardButton.gameObject.SetActive(false);

        HideInstantly();
    }

    private void OnDestroy()
    {
        closeButton.onClick.RemoveAllListeners();
        showUneqippedCardsButton.onClick.RemoveAllListeners();
        takeCardButton.onClick.RemoveAllListeners();
        PlayerPreturn.OnPlayerPreturn -= PlayerPreturn_OnPlayerPreturn;
        AttackPlayerInfoUI.OnShowPlayerEquippedCards -= AttackPlayerInfoUI_OnShowPlayerEquippedCards;
    }

    private void PlayerPreturn_OnPlayerPreturn(object sender, string[] e)
    {
        titleText.text = "PRETURN\nEquipped cards:";

        ShowWithAnimation();

        player = Player.LocalInstance;

        ShowOrHideUnequippedCardsButton();

        InstantiateCards();
    }

    private void AttackPlayerInfoUI_OnShowPlayerEquippedCards(Player obj)
    {
        titleText.text = $"<color=#{obj.HexPlayerColor}>{obj.PlayerName}'s </color> equipped cards:";

        ShowWithAnimation();

        player = obj;

        ShowOrHideUnequippedCardsButton();

        InstantiateCards();
    }

    private void ShowOrHideUnequippedCardsButton()
    {
        if (player == Player.LocalInstance && player.UnequippedCards.Count > 0)
        {
            showUneqippedCardsButton.gameObject.SetActive(true);
        }
        else
        {
            showUneqippedCardsButton.gameObject.SetActive(false);
        }
    }

    private void InstantiateCards()
    {
        foreach (Card card in player.EquippedCards)
        {
            Transform cardUITransform = Instantiate(template, container);

            cardUITransform.gameObject.SetActive(true);

            PlayerCardUI cardUI = cardUITransform.GetComponent<PlayerCardUI>();

            cardUI.Instantiate(card);
        }
    }

    private void Show()
    {
        gameObject.SetActive(true);
        container.gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        container.gameObject.SetActive(false);

        foreach (Transform child in container)
        {
            if (child == template) continue;
            Destroy(child.gameObject);
        }
    }

    public void ShowWithAnimation()
    {
        Show();
        PlayerCardsUIRectTransform.DOScale(Vector2.one, .4f).SetEase(Ease.InOutBack);
    }

    public void HideWithAnimation()
    {
        PlayerCardsUIRectTransform.DOScale(Vector2.zero, .4f).SetEase(Ease.InOutBack).OnComplete(() => Hide());
    }

    public void HideInstantly()
    {
        PlayerCardsUIRectTransform.DOScale(Vector2.zero, .0f).SetEase(Ease.InOutBack).OnComplete(() => Hide());
    }
}
