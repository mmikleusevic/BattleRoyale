using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AttackPlayerInfoUI : MonoBehaviour
{
    public static event Action<OnAttackPlayerEventArgs> OnAttackPlayer;

    public class OnAttackPlayerEventArgs : EventArgs
    {
        public Player player;
        public string message;
    }

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

    public void Instantiate(Player player)
    {
        this.player = player;

        backgroundImage.color = player.HexPlayerColor.HEXToColor();
        playerNameText.text = player.PlayerName;
        pointsText.text = "Points: " + player.Points.ToString();

        equippedCardsText.text += '\n';

        if(player.EquippedCards.Count == 0)
        {
            equippedCardsText.text += "None";
        }

        foreach (Card card in player.EquippedCards)
        {
            equippedCardsText.text += card.Name;

            if(card != player.EquippedCards.LastOrDefault())
            {
                equippedCardsText.text += '\n';
            }
        }

        attackPlayerButton.onClick.AddListener(() =>
        {
            OnAttackPlayer?.Invoke(new OnAttackPlayerEventArgs
            {
                player = this.player,
                message = CreateMessageForMessageUI()
            });
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
