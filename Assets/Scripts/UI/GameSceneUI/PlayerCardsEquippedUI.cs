using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;
using Random = UnityEngine.Random;

public class PlayerCardsEquippedUI : MonoBehaviour
{
    public static event Action OnPreturnCardsInstantiated;
    public static event Action OnPlayerCardsEquippedUIClosed;
    public static event Action OnShowUnequippedCards;
    public static event Action<ulong> OnWonEquippedCard;

    [SerializeField] private RectTransform PlayerCardsUIRectTransform;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Transform container;
    [SerializeField] private Transform template;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button showUneqippedCardsButton;
    [SerializeField] private Button takeCardButton;
    [SerializeField] private Image background;

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

            OnPlayerCardsEquippedUIClosed?.Invoke();
        });

        showUneqippedCardsButton.onClick.AddListener(() =>
        {
            OnShowUnequippedCards?.Invoke();
        });

        takeCardButton.onClick.AddListener(() =>
        {
            StartCoroutine(TakeCard());
        });

        PlayerPreturn.OnPlayerPreturn += PlayerPreturn_OnPlayerPreturn;
        PlayerInfoUI.OnShowPlayerEquippedCards += PlayerInfoUI_OnShowPlayerEquippedCards;
        PlayerBattleResults.OnBattleWin += PlayerBattleResults_OnAfterBattle;
        PlayerPreturn.OnPlayerPreturnOver += PlayerPreturn_OnPlayerPreturnOver;
        Player.OnCardsSwapped += Player_OnCardsSwapped;

        takeCardButton.gameObject.SetActive(false);

        HideInstantly();
    }

    private void OnDisable()
    {
        player = null;
    }

    private void OnDestroy()
    {
        closeButton.onClick.RemoveAllListeners();
        showUneqippedCardsButton.onClick.RemoveAllListeners();
        takeCardButton.onClick.RemoveAllListeners();

        PlayerPreturn.OnPlayerPreturn -= PlayerPreturn_OnPlayerPreturn;
        PlayerInfoUI.OnShowPlayerEquippedCards -= PlayerInfoUI_OnShowPlayerEquippedCards;
        PlayerBattleResults.OnBattleWin -= PlayerBattleResults_OnAfterBattle;
        PlayerPreturn.OnPlayerPreturnOver -= PlayerPreturn_OnPlayerPreturnOver;
        Player.OnCardsSwapped -= Player_OnCardsSwapped;
    }

    private void PlayerPreturn_OnPlayerPreturn(object sender, string[] e)
    {
        titleText.text = "PRETURN(PRESS CARD TO SWAP IT, NEED TO HAVE 3 EQUIPED IF POSSIBLE)\nEQUIPPED CARDS:";

        player = Player.LocalInstance;

        ShowOrHideCloseButton();

        ShowOrHideUnequippedCardsButton(false);

        background.color = player.HexPlayerColor.HEXToColor();

        ShowWithAnimation();

        InstantiateCards();

        OnPreturnCardsInstantiated?.Invoke();
    }

    private void PlayerInfoUI_OnShowPlayerEquippedCards(Player obj, bool isOver)
    {
        titleText.text = $"{obj.PlayerName}'s </color>EQUIPPED CARDS:";

        player = obj;

        closeButton.gameObject.SetActive(true);
        ShowOrHideUnequippedCardsButton(isOver);

        background.color = player.HexPlayerColor.HEXToColor();

        ShowWithAnimation();

        InstantiateCards();
    }

    private void PlayerBattleResults_OnAfterBattle(Player loser)
    {
        player = loser;

        closeButton.gameObject.SetActive(false);
        ShowOrHideUnequippedCardsButton(false);
        takeCardButton.gameObject.SetActive(true);

        background.color = player.HexPlayerColor.HEXToColor();

        ShowWithAnimation();

        InstantiateCards();
    }

    private void PlayerPreturn_OnPlayerPreturnOver(object sender, EventArgs e)
    {
        showUneqippedCardsButton.gameObject.SetActive(false);
    }

    private void Player_OnCardsSwapped(string[] obj)
    {
        ShowOrHideCloseButton();
        ShowOrHideUnequippedCardsButton(false);
    }

    private void ShowOrHideUnequippedCardsButton(bool isOver)
    {
        if (player == Player.LocalInstance && player.UnequippedCards.Count > 0 || isOver)
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
        for (int i = 0; i < player.MaxEquippableCards; i++)
        {
            Transform cardUITransform = Instantiate(template, container);

            cardUITransform.gameObject.SetActive(true);

            PlayerCardUI playerCardUI = cardUITransform.GetComponent<PlayerCardUI>();

            if (player.EquippedCards.Count >= i + 1)
            {
                Card card = player.EquippedCards[i];

                playerCardUI.Instantiate(card, i);
            }
            else
            {
                playerCardUI.Instantiate(i);
            }
        }
    }

    private void ShowOrHideCloseButton()
    {
        if (Player.LocalInstance.UnequippedCards.Count > 0 && Player.LocalInstance.EquippedCards.Count < Player.LocalInstance.MaxEquippableCards)
        {
            closeButton.gameObject.SetActive(false);
        }
        else
        {
            closeButton.gameObject.SetActive(true);
        }
    }

    private IEnumerator TakeCard()
    {
        takeCardButton.gameObject.SetActive(false);

        int randomCardNumber = Random.Range(0, player.EquippedCards.Count);

        Card card = player.EquippedCards[randomCardNumber];

        titleText.text = $"YOU TOOK {card.Name} FROM {player.PlayerName}";

        foreach (Transform child in container)
        {
            PlayerCardUI playerCardUI = child.GetComponent<PlayerCardUI>();
            if (child == template || playerCardUI.Index == randomCardNumber) continue;
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

        foreach (Transform child in container)
        {
            if (child == template)
            {
                child.gameObject.SetActive(false);
                continue;
            }

            child.gameObject.SetActive(true);
        }
    }

    private void Hide()
    {
        gameObject.SetActive(false);

        foreach (Transform child in container)
        {
            if (child == template) continue;
            Destroy(child.gameObject);
        }
    }

    private void ShowWithAnimation()
    {
        Show();
        PlayerCardsUIRectTransform.DOScale(Vector2.one, .4f).SetEase(Ease.InOutBack);
    }

    private void HideWithAnimation()
    {
        PlayerCardsUIRectTransform.DOScale(Vector2.zero, .4f).SetEase(Ease.InOutBack).OnComplete(() => Hide());
    }

    private void HideInstantly()
    {
        PlayerCardsUIRectTransform.DOScale(Vector2.zero, .0f).SetEase(Ease.InOutBack).OnComplete(() => Hide());
    }
}
