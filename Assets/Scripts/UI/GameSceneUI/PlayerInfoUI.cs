using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoUI : MonoBehaviour
{
    public static event Action<NetworkObjectReference, NetworkObjectReference, string> OnAttackPlayer;
    public static event Action<Player> OnShowPlayerEquippedCards;

    private Player player;

    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI playerStatsText;
    [SerializeField] private TextMeshProUGUI equippedCardsText;
    [SerializeField] private TextMeshProUGUI unequippedCardsText;
    [SerializeField] private Button showEquippedCardsButton;
    [SerializeField] private Button attackPlayerButton;

    private void Awake()
    {
        showEquippedCardsButton.onClick.AddListener(() =>
        {
            OnShowPlayerEquippedCards?.Invoke(player);
        });

        Hide();
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

    private void OnDestroy()
    {
        attackPlayerButton.onClick.RemoveAllListeners();
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
