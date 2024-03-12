using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

public class CardBattleResults : NetworkBehaviour
{
    public static event Action OnCardRoll;
    public static event Action<OnCardBattleEventArgs> OnCardWon;
    public static event Action<OnCardBattleEventArgs> OnCardLost;

    public class OnCardBattleEventArgs : EventArgs
    {
        public string[] messages;
        public Card card;
    }

    private Tile tile;

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
        int sum = results.Sum();

        bool isThreeOfAKind = results.Distinct().Count() == 1;

        string[] message = null;

        if (sum >= tile.Card.Value || isThreeOfAKind)
        {
            tile.DisableCard();
            message = SendCardWonMessageToMessageUI();

            OnCardWon?.Invoke(new OnCardBattleEventArgs
            {
                card = tile.Card,
                messages = message,
            });
        }
        else
        {
            message = SendCardLostMessageToMessageUI();
            OnCardLost?.Invoke(new OnCardBattleEventArgs
            {
                card = tile.Card,
                messages = message,
            });
        }

        tile = null;
    }

    private string[] SendCardWonMessageToMessageUI()
    {
        Player player = Player.LocalInstance;

        string playerName = player.PlayerName;
        string playerColor = player.HexPlayerColor;

        return new string[] {
            $"YOU WON {tile.GetCardOrTileName()}",
            $"<color=#{playerColor}>{playerName}</color> won {tile.GetCardOrTileName()}"
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
