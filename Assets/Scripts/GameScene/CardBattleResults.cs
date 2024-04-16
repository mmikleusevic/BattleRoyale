using System;
using System.Linq;
using Unity.Netcode;

public class CardBattleResults : NetworkBehaviour, ICardResults
{
    public static event Action OnCardRoll;
    public static event Action OnCardWon;
    public static event Action OnCardLost;
    public static event Action<Card, Player> OnCardWonCurse;

    private ClientRpcParams callerClientRpcParams;
    private Tile tile;

    public override void OnNetworkSpawn()
    {
        ActionsUI.OnAttackCard += ActionsUI_OnAttackCardServerRpc;

        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        ActionsUI.OnAttackCard -= ActionsUI_OnAttackCardServerRpc;

        base.OnNetworkDespawn();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ActionsUI_OnAttackCardServerRpc(NetworkObjectReference tileNetowkrObjectReference, NetworkObjectReference playerNetworkObjectReference)
    {
        Tile tile = Tile.GetTileFromNetworkReference(tileNetowkrObjectReference);
        this.tile = tile;

        Player player = Player.GetPlayerFromNetworkReference(playerNetworkObjectReference);

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

    public void SetResult(int[] results, int result, RollTypeEnum rollTypeEnum)
    {
        if (rollTypeEnum != RollTypeEnum.CardAttack) return;

        SetResultServerRpc(results, result);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetResultServerRpc(int[] results, int result, ServerRpcParams serverRpcParams = default)
    {
        bool isThreeOfAKind = results.Distinct().Count() == 1;

        if (result >= tile.Card.WinValue || isThreeOfAKind)
        {
            CardWonLogicClientRpc(isThreeOfAKind, callerClientRpcParams);
        }
        else
        {
            CardLostLogicClientRpc(callerClientRpcParams);
        }

        CardAbilities.ResetRerolls();

        tile = null;
    }

    [ClientRpc]
    private void CardWonLogicClientRpc(bool isThreeOfAKind, ClientRpcParams clientRpcParams = default)
    {
        Player player = Player.LocalInstance;
        if (isThreeOfAKind)
        {
            SendThreeOfAKindMessageToMessageUI();
        }

        string[] messages = SendCardWonMessageToMessageUI();

        if (player.EquippedCards.Count >= player.MaxEquippableCards && tile.Card.Ability != null && tile.Card.Ability is ICurse)
        {
            OnCardWonCurse?.Invoke(tile.Card, null);
        }
        else
        {
            Player.LocalInstance.SaveWonCard(tile.Card);
        }

        OnCardWon?.Invoke();

        tile.DisableCard();
        tile.RemoveCardServerRpc();

        MessageUI.Instance.SendMessageToEveryoneExceptMe(messages);
        FadeMessageUI.Instance.StartFadeMessage(messages[0]);
    }

    [ClientRpc]
    private void CardLostLogicClientRpc(ClientRpcParams clientRpcParams = default)
    {
        string[] messages = SendCardLostMessageToMessageUI();

        Player.LocalInstance.PlayerDiedCardBattle();

        OnCardLost?.Invoke();

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

    public static void ResetStaticData()
    {
        OnCardLost = null;
        OnCardRoll = null;
        OnCardWon = null;
        OnCardWonCurse = null;
    }
}
