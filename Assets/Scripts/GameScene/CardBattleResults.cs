using System;
using System.Linq;
using Unity.Netcode;

public class CardBattleResults : NetworkBehaviour, ICardResults
{
    public static event Action OnCardRoll;
    public static event Action<Card> OnCardWon;
    public static event Action<Card> OnCardLost;

    private ClientRpcParams callerClientRpcParams;
    private Tile tile;

    private void Awake()
    {
        ActionsUI.OnAttackCard += ActionsUI_OnAttackCardServerRpc;
    }

    public override void OnDestroy()
    {
        ActionsUI.OnAttackCard -= ActionsUI_OnAttackCardServerRpc;

        base.OnDestroy();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ActionsUI_OnAttackCardServerRpc(NetworkObjectReference tileNetowkrObjectReference, NetworkObjectReference playerNetworkObjectReference)
    {
        Tile tile = Tile.GetTileFromNetworkReference(tileNetowkrObjectReference);
        Player player = Player.GetPlayerFromNetworkReference(playerNetworkObjectReference);

        this.tile = tile;

        ulong[] clientId = new ulong[] { player.ClientId.Value };

        callerClientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = clientId
            }
        };

        CallForRollClientRpc(tileNetowkrObjectReference, callerClientRpcParams);
    }

    [ClientRpc]
    private void CallForRollClientRpc(NetworkObjectReference tileNetowkrObjectReference, ClientRpcParams clientRpcParams = default)
    {
        Tile tile = Tile.GetTileFromNetworkReference(tileNetowkrObjectReference);

        this.tile = tile;

        RollType.rollType = RollTypeEnum.CardAttack;

        OnCardRoll?.Invoke();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetResultServerRpc(int[] results, int result, RollTypeEnum rollTypeEnum, ServerRpcParams serverRpcParams = default)
    {
        if (rollTypeEnum != RollTypeEnum.CardAttack) return;

        bool isThreeOfAKind = results.Distinct().Count() == 1;

        if (result >= tile.Card.WinValue || isThreeOfAKind)
        {
            CardWonLogicClientRpc(isThreeOfAKind, callerClientRpcParams);
        }
        else
        {
            CardLostLogicClientRpc(callerClientRpcParams);
        }

        tile = null;
    }

    [ClientRpc]
    private void CardWonLogicClientRpc(bool isThreeOfAKind, ClientRpcParams clientRpcParams = default)
    {
        if (isThreeOfAKind)
        {
            SendThreeOfAKindMessageToMessageUI();
        }

        tile.DisableCard();

        string[] messages = SendCardWonMessageToMessageUI();

        Player.LocalInstance.SaveWonCard(tile.Card);

        OnCardWon?.Invoke(tile.Card);

        MessageUI.Instance.SendMessageToEveryoneExceptMe(messages);
        FadeMessageUI.Instance.StartFadeMessage(messages[0]);
    }

    [ClientRpc]
    private void CardLostLogicClientRpc(ClientRpcParams clientRpcParams = default)
    {
        string[] messages = SendCardLostMessageToMessageUI();

        Player.LocalInstance.PlayerDiedCardBattle();

        OnCardLost?.Invoke(tile.Card);

        MessageUI.Instance.SendMessageToEveryoneExceptMe(messages);
        FadeMessageUI.Instance.StartFadeMessage(messages[0]);
    }

    private void SendThreeOfAKindMessageToMessageUI()
    {
        string message = $"<color=#{Player.LocalInstance.HexPlayerColor}>{Player.LocalInstance.PlayerName}</color> rolled THREE OF A KIND";

        MessageUI.Instance.SendMessageToEveryoneExceptMe(message);
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
