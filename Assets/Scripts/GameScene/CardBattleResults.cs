using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

public class CardBattleResults : NetworkBehaviour
{
    public static event Action OnCardRoll;
    public static event Action<OnCardWonEventArgs> OnCardWon;
    public static event Action<string> OnCardLost;
    public static event EventHandler<string> OnCardRollOver;

    public class OnCardWonEventArgs : EventArgs
    {
        public string message;
        public Card card;
    }

    private Card card;
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

    private void ActionsUI_OnAttackCard(Card card)
    {
        this.card = card;

        RollType.rollType = RollTypeEnum.CardAttack;

        OnCardRoll?.Invoke();
    }

    public void SetCardResult(List<int> results)
    {
        cardRolls = results;

        int sum = cardRolls.Sum();

        bool isThreeOfAKind = cardRolls.Distinct().Count() == 1;

        if (sum >= card.Value || isThreeOfAKind)
        {
            card.DisableCard();
            OnCardWon?.Invoke(new OnCardWonEventArgs
            {
                card = card,
                message = SendCardWonMessageToMessageUI(),
            });
        }
        else
        {
            OnCardLost?.Invoke(SendCardLostMessageToMessageUI());
        }

        cardRolls.Clear();
        card = null;
    }

    private string SendCardWonMessageToMessageUI()
    {
        Player player = Player.LocalInstance;

        string playerName = player.PlayerName;
        string playerColor = player.HexPlayerColor;

        return $"<color=#{playerColor}>{playerName}</color> won {card.Name}";
    }

    private string SendCardLostMessageToMessageUI()
    {
        Player player = Player.LocalInstance;

        string playerName = player.PlayerName;
        string playerColor = player.HexPlayerColor;

        return $"<color=#{playerColor}>{playerName}</color> has failed a card roll and died.";
    }
}
