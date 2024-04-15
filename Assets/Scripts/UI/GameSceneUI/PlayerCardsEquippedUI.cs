using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;
using Random = UnityEngine.Random;

public class PlayerCardsEquippedUI : MonoBehaviour
{
    public static event Action OnPreturnCardsInstantiated;
    public static event Action OnPlayerCardsEquippedUIClosed;
    public static event Action<Player> OnShowUnequippedCards;
    public static event Action<ulong> OnWonEquippedCard;
    public static event Action OnPreturnOver;

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
            if (StateManager.Instance.GetCurrentState() == StateEnum.PlayerPreturn)
            {
                await StateManager.Instance.EndState();
            }

            if (PlayerCardUI.equippedCardState == EquippedCardState.Disarm)
            {
                OnPreturnOver?.Invoke();
            }
            else
            {
                OnPlayerCardsEquippedUIClosed?.Invoke();
            }

            HideWithAnimation();
        });

        showUneqippedCardsButton.onClick.AddListener(() =>
        {
            OnShowUnequippedCards?.Invoke(player);
        });

        takeCardButton.onClick.AddListener(() =>
        {
            StartCoroutine(TakeCard());
        });

        PlayerPreturn.OnPlayerPreturn += PlayerPreturn_OnPlayerPreturn;
        PlayerInfoUI.OnShowPlayerEquippedCards += PlayerInfoUI_OnShowPlayerEquippedCards;
        PlayerBattleResults.OnBattleWin += PlayerBattleResults_OnAfterBattle;
        PlayerPreturn.OnPlayerPreturnOver += PlayerPreturn_OnPlayerPreturnOver;
        PlayerBattleResults.OnPrebattle += PlayerBattleResults_OnPrebattle;
        PlayerBattleResults.OnPlayerBattleSet += PlayerBattleResults_OnPlayerBattleSet;
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
        PlayerBattleResults.OnPrebattle -= PlayerBattleResults_OnPrebattle;
        PlayerBattleResults.OnPlayerBattleSet -= PlayerBattleResults_OnPlayerBattleSet;
    }

    private void PlayerPreturn_OnPlayerPreturn()
    {
        Hide();

        titleText.text = "PRETURN(PRESS CARD TO SWAP IT, NEED TO HAVE 3 EQUIPED IF POSSIBLE)\nEQUIPPED CARDS:";

        player = Player.LocalInstance;

        ShowOrHideCloseButton();

        ShowOrHideUnequippedCardsButton(false);

        background.color = player.HexPlayerColor.HEXToColor();

        ShowWithAnimation();

        if (Player.LocalInstance.UnequippedCards.Count > 0)
        {
            PlayerCardUI.equippedCardState = EquippedCardState.Swap;
        }

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

        PlayerCardUI.equippedCardState = EquippedCardState.None;

        InstantiateCards();
    }

    private void PlayerBattleResults_OnAfterBattle(Player loser)
    {
        player = loser;

        titleText.text = $"TAKE CARD FROM {player.PlayerName}";

        closeButton.gameObject.SetActive(false);
        ShowOrHideUnequippedCardsButton(false);
        takeCardButton.gameObject.SetActive(true);

        background.color = player.HexPlayerColor.HEXToColor();

        ShowWithAnimation();

        PlayerCardUI.equippedCardState = EquippedCardState.None;

        InstantiateCards();
    }

    private void PlayerPreturn_OnPlayerPreturnOver()
    {
        showUneqippedCardsButton.gameObject.SetActive(false);
    }

    private void Player_OnCardsSwapped()
    {
        ShowOrHideCloseButton();
        ShowOrHideUnequippedCardsButton(false);
    }

    private void PlayerBattleResults_OnPrebattle(Player enemy)
    {
        player = enemy;

        titleText.text = $"DISARMING {enemy.PlayerName}'s </color>CARDS (Press to disarm)";

        closeButton.gameObject.SetActive(true);
        ShowOrHideUnequippedCardsButton(false);

        background.color = player.HexPlayerColor.HEXToColor();

        ShowWithAnimation();

        PlayerCardUI.enemy = enemy;
        PlayerCardUI.equippedCardState = EquippedCardState.Disarm;

        InstantiateCards();
    }

    private void PlayerBattleResults_OnPlayerBattleSet()
    {
        HideWithAnimation();
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
        DestroyTransforms();

        for (int i = 0; i < player.MaxEquippableCards; i++)
        {
            Transform cardUITransform = Instantiate(template, container);

            cardUITransform.gameObject.SetActive(true);

            PlayerCardUI playerCardUI = cardUITransform.GetComponent<PlayerCardUI>();

            playerCardUI.gameObject.SetActive(true);

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

        List<int> curseIndexes = player.EquippedCards
                            .Select((card, index) => new { Card = card, Index = index })
                            .Where(x => x.Card is ICurse)
                            .Select(x => x.Index)
                            .ToList();

        int randomCardNumber = Random.Range(0, player.EquippedCards.Count);

        if (curseIndexes.Count > 0)
        {
            while (!curseIndexes.Contains(randomCardNumber))
            {
                randomCardNumber = Random.Range(0, player.EquippedCards.Count);
            }
        }

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

        PlayerCardUI.equippedCardState = EquippedCardState.None;

        DestroyTransforms();
    }

    private void DestroyTransforms()
    {
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

    public static void ResetStaticData()
    {
        OnPreturnCardsInstantiated = null;
        OnPlayerCardsEquippedUIClosed = null;
        OnShowUnequippedCards = null;
        OnWonEquippedCard = null;
        OnPreturnOver = null;
    }
}
