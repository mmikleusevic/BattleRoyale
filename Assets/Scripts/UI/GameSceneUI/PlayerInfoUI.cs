using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoUI : MonoBehaviour
{
    public static event Action<NetworkObjectReference, NetworkObjectReference, string> OnAttackPlayer;
    public static event Action<Player, bool> OnShowPlayerEquippedCards;

    private Player player;

    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI playerStatsText;
    [SerializeField] private TextMeshProUGUI equippedCardsText;
    [SerializeField] private TextMeshProUGUI unequippedCardsText;
    [SerializeField] private Button showEquippedCardsButton;
    [SerializeField] private Button attackPlayerButton;

    private bool isOver = false;

    private void Awake()
    {
        showEquippedCardsButton.onClick.AddListener(() =>
        {
            OnShowPlayerEquippedCards?.Invoke(player, isOver);
        });

        Won.OnWon += OnGameOver;
        Lost.OnLost += OnGameOver;

        Hide();
    }

    private void OnDestroy()
    {
        Won.OnWon -= OnGameOver;
        Lost.OnLost -= OnGameOver;

        showEquippedCardsButton.onClick.RemoveAllListeners();
    }

    private void OnDisable()
    {
        attackPlayerButton.onClick.RemoveAllListeners();
    }

    private void OnGameOver(string obj)
    {
        isOver = true;
    }

    public void Instantiate(Player player, bool showAttackButton)
    {
        this.player = player;

        if (showAttackButton)
        {
            if (player == Player.LocalInstance) return;

            attackPlayerButton.gameObject.SetActive(true);

            attackPlayerButton.onClick.AddListener(() =>
            {
                OnAttackPlayer?.Invoke(
                    Player.LocalInstance.NetworkObject,
                    player.NetworkObject,
                    CreateMessageForMessageUI()
                );
            });
        }
        else
        {
            attackPlayerButton.gameObject.SetActive(false);

            if (player.UnequippedCards.Count <= 0)
            {
                unequippedCardsText.text = "UNEQUIPPED CARDS: NONE";
            }
            else
            {
                unequippedCardsText.text = $"UNEQUIPPED CARDS: {player.UnequippedCards.Count}";
            }
        }

        backgroundImage.color = player.HexPlayerColor.HEXToColor();
        playerNameText.text = player.PlayerName;
        playerStatsText.text = "POINTS: " + player.Points.Value.ToString() + "\n" +
                               "SIPS: " + player.SipCounter;

        if (player.EquippedCards.Count <= 0)
        {
            equippedCardsText.text = "EQUIPPED CARDS: NONE";
            showEquippedCardsButton.gameObject.SetActive(false);
        }
        else
        {
            equippedCardsText.text = $"EQUIPPED CARDS: {player.EquippedCards.Count}";
            showEquippedCardsButton.gameObject.SetActive(true);
        }

        Show();
    }

    private string CreateMessageForMessageUI()
    {
        return $"<color=#{Player.LocalInstance.HexPlayerColor}>{Player.LocalInstance.PlayerName}</color>'s ATTACKING <color=#{player.HexPlayerColor}>{player.PlayerName}</color>";
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
