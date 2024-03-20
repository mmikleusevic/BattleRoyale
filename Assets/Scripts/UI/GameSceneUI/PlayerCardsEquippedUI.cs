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
        titleText.text = "PRETURN(PRESS CARD TO SWAP IT)\nEQUIPPED CARDS:";

        closeButton.gameObject.SetActive(true);
        ShowOrHideUnequippedCardsButton();

        player = Player.LocalInstance;

        background.color = player.HexPlayerColor.HEXToColor();

        ShowWithAnimation();

        InstantiateCards();

        OnPreturnCardsInstantiated?.Invoke();
    }

    private void AttackPlayerInfoUI_OnShowPlayerEquippedCards(Player obj)
    {
        titleText.text = $"{obj.PlayerName}'s </color>EQUIPPED CARDS:";

        ShowOrHideUnequippedCardsButton();

        player = obj;

        background.color = player.HexPlayerColor.HEXToColor();

        ShowWithAnimation();

        InstantiateCards();
    }

    private void PlayerBattleResults_OnAfterBattle(Player loser)
    {
        closeButton.gameObject.SetActive(false);
        ShowOrHideUnequippedCardsButton();
        takeCardButton.gameObject.SetActive(true);

        player = loser;

        background.color = player.HexPlayerColor.HEXToColor();

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
        for (int i = 0; i < player.MaxEquippableCards; i++)
        {
            Transform cardUITransform = Instantiate(template, container);

            cardUITransform.gameObject.SetActive(true);

            PlayerCardUI playerCardUI = cardUITransform.GetComponent<PlayerCardUI>();

            if (player.EquippedCards.Count >= i+1)
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
