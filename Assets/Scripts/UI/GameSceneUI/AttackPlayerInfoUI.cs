using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class AttackPlayerInfoUI : MonoBehaviour
{
    public static event Action<NetworkObjectReference, NetworkObjectReference, string> OnAttackPlayer;
    public static event Action<Player> OnShowPlayerEquippedCards;

    private Player player;

    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI pointsText;
    [SerializeField] private TextMeshProUGUI equippedCardsText;
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

    public void Instantiate(Player enemyPlayer)
    {
        player = enemyPlayer;

        backgroundImage.color = enemyPlayer.HexPlayerColor.HEXToColor();
        playerNameText.text = enemyPlayer.PlayerName;
        pointsText.text = "Points: " + enemyPlayer.Points.Value.ToString();

        if (enemyPlayer.EquippedCards.Count == 0)
        {
            equippedCardsText.text += "None";
        }

        if (enemyPlayer.EquippedCards.Count > 0)
        {
            showEquippedCardsButton.interactable = true;
        }
        else
        {
            showEquippedCardsButton.interactable = false;
        }

        attackPlayerButton.onClick.AddListener(() =>
        {
            OnAttackPlayer?.Invoke(
                Player.LocalInstance.NetworkObject,
                enemyPlayer.NetworkObject,
                CreateMessageForMessageUI()
            );
        });

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
