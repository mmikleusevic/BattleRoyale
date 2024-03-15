using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PlayerCardsUI : MonoBehaviour
{
    public static event Action OnPlayerCardsUIClosed;
    public static event Action<ulong> OnWonEquippedCard;

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
            StartCoroutine(TakeCard());
        });

        PlayerPreturn.OnPlayerPreturn += PlayerPreturn_OnPlayerPreturn;
        AttackPlayerInfoUI.OnShowPlayerEquippedCards += AttackPlayerInfoUI_OnShowPlayerEquippedCards;
        PlayerBattleResults.OnBattleWin += PlayerBattleResults_OnAfterBattle;

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
        PlayerBattleResults.OnBattleWin -= PlayerBattleResults_OnAfterBattle;
    }

    private void PlayerPreturn_OnPlayerPreturn(object sender, string[] e)
    {
        titleText.text = "PRETURN\nEQUIPPED CARDS:";

        closeButton.gameObject.SetActive(true);

        ShowWithAnimation();

        player = Player.LocalInstance;

        ShowOrHideUnequippedCardsButton();

        InstantiateCards();
    }

    private void AttackPlayerInfoUI_OnShowPlayerEquippedCards(Player obj)
    {
        titleText.text = $"<color=#{obj.HexPlayerColor}>{obj.PlayerName}'s </color>EQUIPPED CARDS:";

        ShowWithAnimation();

        player = obj;

        ShowOrHideUnequippedCardsButton();

        InstantiateCards();
    }

    private void PlayerBattleResults_OnAfterBattle(Player loser)
    {
        closeButton.gameObject.SetActive(false);
        showUneqippedCardsButton.gameObject.SetActive(false);
        takeCardButton.gameObject.SetActive(true);

        player = loser;

        ShowWithAnimation();

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
        int i = 0;

        foreach (Card card in player.EquippedCards)
        {
            Transform cardUITransform = Instantiate(template, container);

            cardUITransform.gameObject.SetActive(true);

            PlayerCardUI playerCardUI = cardUITransform.GetComponent<PlayerCardUI>();

            playerCardUI.Instantiate(card, i);

            i++;
        }
    }

    private IEnumerator TakeCard()
    {
        takeCardButton.gameObject.SetActive(false);

        int randomCardNumber = Random.Range(0, player.EquippedCards.Count);

        Card card = player.EquippedCards[randomCardNumber];

        titleText.text = $"YOU TOOK {card.Name} FROM <color=#{player.HexPlayerColor}>{player.PlayerName}</color>";

        foreach (Transform child in container)
        {
            PlayerCardUI playerCardUI = child.GetComponent<PlayerCardUI>();
            if (child == template || playerCardUI.index == randomCardNumber) continue;
            Destroy(child.gameObject);
        }

        Player.LocalInstance.OnBattleWon(card, player);

        OnWonEquippedCard?.Invoke(Player.LocalInstance.ClientId.Value);

        yield return new WaitForSeconds(2f);

        HideWithAnimation();
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
