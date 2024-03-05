using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

public class CardBattleResults : NetworkBehaviour
{
    public static event Action OnCardRoll;
    public static event Action<OnCardWonEventArgs> OnCardWon;
    public static event Action<string[]> OnCardLost;
    public static event EventHandler<string> OnCardRollOver;

    public class OnCardWonEventArgs : EventArgs
    {
        public string[] messages;
        public Card card;
    }

    private Tile tile;
    private List<int> cardRolls;

    private void Awake()
    {
        ActionsUI.OnAttackCard += ActionsUI_OnAttackCard;
    }

    public override void OnDestroy()
    {
        ActionsUI.OnAttackCard -= ActionsUI_OnAttackCard;

        base.OnDestroy();
    }

    private void ActionsUI_OnAttackCard(Tile tile, string[] messages)
    {
        this.tile = tile;

        RollType.rollType = RollTypeEnum.CardAttack;

        OnCardRoll?.Invoke();
    }

    public void SetCardResult(List<int> results)
    {
        cardRolls = results;

        int sum = cardRolls.Sum();

        bool isThreeOfAKind = cardRolls.Distinct().Count() == 1;

        string[] message = null;

        if (sum >= tile.Card.Value || isThreeOfAKind)
        {
            tile.DisableCard();
            message = SendCardWonMessageToMessageUI();

            OnCardWon?.Invoke(new OnCardWonEventArgs
            {
                card = tile.Card,
                messages = message,
            });
        }
        else
        {
            message = SendCardLostMessageToMessageUI();
            OnCardLost?.Invoke(message);
        }

        cardRolls.Clear();
        tile = null;
    }

    private string[] SendCardWonMessageToMessageUI()
    {
        Player player = Player.LocalInstance;

        string playerName = player.PlayerName;
        string playerColor = player.HexPlayerColor;

        return new string[] {
            $"YOU WON {tile.Card.Name}",
            $"<color=#{playerColor}>{playerName}</color> won {tile.Card.Name}"
        };
    }

    private string[] SendCardLostMessageToMessageUI()
    {
        Player player = Player.LocalInstance;

        string playerName = player.PlayerName;
        string playerColor = player.HexPlayerColor;

        return new string[] {
            "YOU DIED",
            $"<color=#{playerColor}>{playerName}</color> has failed a card roll and died."
        };
    }
}
