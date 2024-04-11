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
        Player.OnCardsSwapped += Player_OnCardsSwapped;
        PlayerInfoUI.OnDisarm += PlayerInfoUI_OnDisarm;
        PlayerCardUI.OnDisarmOver += PlayerCardUI_OnDisarmOver;
        Won.OnWon += OnGameOver;
        Lost.OnLost += OnGameOver;

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
        PlayerInfoUI.OnDisarm -= PlayerInfoUI_OnDisarm;
        PlayerCardUI.OnDisarmOver -= PlayerCardUI_OnDisarmOver;
        Won.OnWon -= OnGameOver;
        Lost.OnLost -= OnGameOver;
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

        EquippedCardState equippedCardState = EquippedCardState.None;

        if (Player.LocalInstance.UnequippedCards.Count > 0)
        {
            equippedCardState = EquippedCardState.Swap;
        }

        InstantiateCards(equippedCardState);

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

        InstantiateCards(EquippedCardState.None);
    }

    private void PlayerBattleResults_OnAfterBattle(Player loser)
    {
        player = loser;

        closeButton.gameObject.SetActive(false);
        ShowOrHideUnequippedCardsButton(false);
        takeCardButton.gameObject.SetActive(true);

        background.color = player.HexPlayerColor.HEXToColor();

        ShowWithAnimation();

        InstantiateCards(EquippedCardState.None);
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

    private void PlayerInfoUI_OnDisarm(Player enemy)
    {
        player = enemy;

        titleText.text = $"DISARMING {enemy.PlayerName}'s </color>CARDS (Press to disarm)";

        closeButton.gameObject.SetActive(false);
        ShowOrHideUnequippedCardsButton(false);

        background.color = player.HexPlayerColor.HEXToColor();

        PlayerCardUI.enemy = enemy;

        ShowWithAnimation();

        InstantiateCards(EquippedCardState.Disarm);
    }

    private void PlayerCardUI_OnDisarmOver(Player player, Player enemy)
    {
        HideWithAnimation();
    }

    private void OnGameOver(string obj)
    {
        PlayerPreturn.OnPlayerPreturn -= PlayerPreturn_OnPlayerPreturn;
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

    private void InstantiateCards(EquippedCardState equippedCardState)
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

                playerCardUI.Instantiate(card, i, equippedCardState);
            }
            else
            {
                playerCardUI.Instantiate(i, equippedCardState);
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
}
