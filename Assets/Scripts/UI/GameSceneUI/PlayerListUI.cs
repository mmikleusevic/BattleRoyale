using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerListUI : MonoBehaviour
{
    [SerializeField] private Transform container;
    [SerializeField] private Transform template;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button infoButton;
    [SerializeField] private TextMeshProUGUI gameOverText;
    private int originalSiblingIndex;

    private void Awake()
    {
        closeButton.onClick.AddListener(() => Hide());
        infoButton.onClick.AddListener(() => ShowPlayersInfo());
        infoButton.gameObject.SetActive(false);

        ActionsUI.OnAttackPlayer += ActionsUI_OnAttackPlayer;
        PlayerBattleResults.OnPlayerBattleSet += PlayerBattleResults_OnPlayerBattleSet;
        PlayerInfoUI.OnShowPlayerEquippedCards += PlayerInfoUI_OnShowPlayerEquippedCards;
        PlayerInfoUI.OnAttackPlayer += PlayerInfoUI_OnAttackPlayer;
        PlayerBattleResults.OnPrebattle += PlayerBattleResults_OnPrebattle;
        CardBattleResults.OnCardWonCurse += CardBattleResults_OnCardWonCurse;
        PlayerCardsEquippedUI.OnPlayerCardsEquippedUIClosed += PlayerCardsEquippedUI_OnPlayerCardsUIClosed;
        PlayerManager.OnActivePlayerChanged += PlayerManager_OnActivePlayerChanged;
        Won.OnWon += OnGameOver;
        Lost.OnLost += OnGameOver;
        PlayerCardUI.OnPrebattleOver += PlayerCardUI_OnPrebattleOver;

        gameOverText.gameObject.SetActive(false);
        originalSiblingIndex = transform.GetSiblingIndex();
    }

    private void OnDestroy()
    {
        closeButton.onClick.RemoveAllListeners();
        infoButton.onClick.RemoveAllListeners();

        ActionsUI.OnAttackPlayer -= ActionsUI_OnAttackPlayer;
        PlayerBattleResults.OnPlayerBattleSet -= PlayerBattleResults_OnPlayerBattleSet;
        PlayerInfoUI.OnShowPlayerEquippedCards -= PlayerInfoUI_OnShowPlayerEquippedCards;
        PlayerInfoUI.OnAttackPlayer -= PlayerInfoUI_OnAttackPlayer;
        PlayerBattleResults.OnPrebattle -= PlayerBattleResults_OnPrebattle;
        CardBattleResults.OnCardWonCurse -= CardBattleResults_OnCardWonCurse;
        PlayerCardsEquippedUI.OnPlayerCardsEquippedUIClosed -= PlayerCardsEquippedUI_OnPlayerCardsUIClosed;
        PlayerManager.OnActivePlayerChanged -= PlayerManager_OnActivePlayerChanged;
        Won.OnWon -= OnGameOver;
        Lost.OnLost -= OnGameOver;
        PlayerCardUI.OnPrebattleOver -= PlayerCardUI_OnPrebattleOver;
    }

    private void ActionsUI_OnAttackPlayer(Tile tile)
    {
        RestoreOriginalOrder();

        Show();

        List<Player> players = tile.GetAlivePlayersOnCard();

        InstantiatePlayerInfo(players, true);
    }

    private void ShowPlayersInfo()
    {
        SetAsLastSibling();

        List<Player> players = PlayerManager.Instance.Players;

        InstantiatePlayerInfo(players, false);

        Show();
    }

    private void InstantiatePlayerInfo(List<Player> players, bool showAttackButton)
    {
        foreach (Player player in players)
        {
            Transform cardTransform = Instantiate(template, container);

            cardTransform.gameObject.SetActive(true);

            PlayerInfoUI playerInfoUI = cardTransform.GetComponent<PlayerInfoUI>();

            playerInfoUI.Instantiate(player, showAttackButton);
        }
    }

    private void PlayerBattleResults_OnPlayerBattleSet()
    {
        RestoreOriginalOrder();

        Hide();
    }

    private void PlayerInfoUI_OnShowPlayerEquippedCards(Player obj, bool isOver)
    {
        RestoreOriginalOrder();
    }

    private void PlayerInfoUI_OnAttackPlayer(NetworkObjectReference player, NetworkObjectReference enemy)
    {
        RestoreOriginalOrder();

        Hide();
    }

    private void PlayerBattleResults_OnPrebattle(Player obj)
    {
        Show();

        RestoreOriginalOrder();
    }

    private void CardBattleResults_OnCardWonCurse(Card card, Player emptyPlayer)
    {
        Show();

        RestoreOriginalOrder();
    }

    private void PlayerCardsEquippedUI_OnPlayerCardsUIClosed()
    {
        StateEnum stateEnum = StateManager.Instance.GetCurrentState();

        if (stateEnum == StateEnum.Won || stateEnum == StateEnum.Lost)
        {
            RestoreOriginalOrder();
        }
        else if (container.childCount > 1)
        {
            SetAsLastSibling();
        }
    }

    private void PlayerManager_OnActivePlayerChanged(Player obj)
    {
        infoButton.gameObject.SetActive(true);

        PlayerManager.OnActivePlayerChanged -= PlayerManager_OnActivePlayerChanged;
    }

    private void OnGameOver(string gameOverText)
    {
        Won.OnWon -= OnGameOver;
        Lost.OnLost -= OnGameOver;

        this.gameOverText.gameObject.SetActive(true);
        closeButton.gameObject.SetActive(false);
        infoButton.gameObject.SetActive(false);

        this.gameOverText.text = gameOverText;

        RestoreOriginalOrder();

        List<Player> players = PlayerManager.Instance.Players;

        InstantiatePlayerInfo(players, false);
    }

    private void PlayerCardUI_OnPrebattleOver()
    {
        RestoreOriginalOrder();

        Hide();
    }

    private void SetAsLastSibling()
    {
        if (gameObject.activeInHierarchy)
        {
            transform.SetAsLastSibling();
        }
    }

    public void RestoreOriginalOrder()
    {
        transform.SetSiblingIndex(originalSiblingIndex);
    }

    private void Show()
    {
        transform.SetAsLastSibling();

        foreach (Transform child in container)
        {
            if (child == template) continue;

            child.gameObject.SetActive(true);
        }
    }

    public void Hide()
    {
        RestoreOriginalOrder();

        foreach (Transform child in container)
        {
            if (child == template) continue;
            Destroy(child.gameObject);
        }
    }
}
