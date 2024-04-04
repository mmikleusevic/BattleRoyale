using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

public class CardBattleResults : NetworkBehaviour
{
    public static event Action OnCardRoll;
    public static event Action<Card> OnCardWon;
    public static event Action<Card> OnCardLost;

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

    private void ActionsUI_OnAttackCard(Tile tile)
    {
        this.tile = tile;

        RollType.rollType = RollTypeEnum.CardAttack;

        OnCardRoll?.Invoke();
    }

    public void SetCardResult(List<int> results, int result)
    {
        bool isThreeOfAKind = results.Distinct().Count() == 1;

        string[] messages = null;

        if (result >= tile.Card.WinValue || isThreeOfAKind)
        {
            tile.DisableCard();
            messages = SendCardWonMessageToMessageUI();

            Player.LocalInstance.SaveWonCard(tile.Card);

            OnCardWon?.Invoke(tile.Card);
        }
        else
        {
            messages = SendCardLostMessageToMessageUI();

            Player.LocalInstance.PlayerDiedCardBattle();

            OnCardLost?.Invoke(tile.Card);
        }

        MessageUI.Instance.SendMessageToEveryoneExceptMe(messages);
        FadeMessageUI.Instance.StartFadeMessage(messages[0]);

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
