using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerListUI : MonoBehaviour
{
    [SerializeField] private Transform container;
    [SerializeField] private Transform template;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button infoButton;
    private int originalSiblingIndex;

    private void Awake()
    {
        closeButton.onClick.AddListener(() => Hide());
        infoButton.onClick.AddListener(() => ShowPlayersInfo());
        infoButton.gameObject.SetActive(false);

        ActionsUI.OnAttackPlayer += ActionsUI_OnAttackPlayer;
        PlayerInfoUI.OnAttackPlayer += PlayerInfoUI_OnAttackPlayer;
        PlayerInfoUI.OnShowPlayerEquippedCards += PlayerInfoUI_OnShowPlayerEquippedCards;
        PlayerCardsEquippedUI.OnPlayerCardsEquippedUIClosed += PlayerCardsUI_OnPlayerCardsUIClosed;
        PlayerManager.Instance.OnActivePlayerChanged += Instance_OnActivePlayerChanged;

        originalSiblingIndex = transform.GetSiblingIndex();
    }

    private void OnDestroy()
    {
        closeButton.onClick.RemoveAllListeners();
        infoButton.onClick.RemoveAllListeners();

        ActionsUI.OnAttackPlayer -= ActionsUI_OnAttackPlayer;      
        PlayerInfoUI.OnAttackPlayer -= PlayerInfoUI_OnAttackPlayer;
        PlayerInfoUI.OnShowPlayerEquippedCards -= PlayerInfoUI_OnShowPlayerEquippedCards;
        PlayerCardsEquippedUI.OnPlayerCardsEquippedUIClosed -= PlayerCardsUI_OnPlayerCardsUIClosed;
    }

    private void ActionsUI_OnAttackPlayer(Tile tile)
    {
        RestoreOriginalOrder();

        Show();

        List<Player> players = tile.GetPlayersOnCard();

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

    private void PlayerInfoUI_OnAttackPlayer(NetworkObjectReference arg1, NetworkObjectReference arg2, string arg3)
    {
        RestoreOriginalOrder();

        Hide();
    }

    private void PlayerInfoUI_OnShowPlayerEquippedCards(Player obj)
    {
        RestoreOriginalOrder();
    }

    private void PlayerCardsUI_OnPlayerCardsUIClosed()
    {
        if (container.childCount > 1)
        {
            SetAsLastSibling();
        }
        else
        {
            RestoreOriginalOrder();
        }
    }

    private void Instance_OnActivePlayerChanged(Player obj)
    {
        infoButton.gameObject.SetActive(true);

        PlayerManager.Instance.OnActivePlayerChanged -= Instance_OnActivePlayerChanged;
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
