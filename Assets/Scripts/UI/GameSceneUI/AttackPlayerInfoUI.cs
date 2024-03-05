using System;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class AttackPlayerInfoUI : MonoBehaviour
{
    public static event Action<NetworkObjectReference, NetworkObjectReference, string> OnAttackPlayer;

    private Player player;

    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI pointsText;
    [SerializeField] private TextMeshProUGUI equippedCardsText;
    [SerializeField] private Button attackPlayerButton;

    private void Awake()
    {
        Hide();
    }

    public void Instantiate(Player enemyPlayer)
    {
        player = enemyPlayer;

        backgroundImage.color = enemyPlayer.HexPlayerColor.HEXToColor();
        playerNameText.text = enemyPlayer.PlayerName;
        pointsText.text = "Points: " + enemyPlayer.Points.ToString();

        equippedCardsText.text += '\n';

        if (enemyPlayer.EquippedCards.Count == 0)
        {
            equippedCardsText.text += "None";
        }

        foreach (Card card in enemyPlayer.EquippedCards)
        {
            equippedCardsText.text += card.Name;

            if (card != enemyPlayer.EquippedCards.LastOrDefault())
            {
                equippedCardsText.text += '\n';
            }
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
        return $"<color=#{Player.LocalInstance.HexPlayerColor}>{Player.LocalInstance.PlayerName}</color> is ATTACKING <color=#{player.HexPlayerColor}>{player.PlayerName}</color>";
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
