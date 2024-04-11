using Unity.Netcode;

public class DisarmAbility : NetworkBehaviour, IDisarm
{
    private Disarm disarm;

    public override void OnNetworkSpawn()
    {
        disarm = GetComponent<Disarm>();

        base.OnNetworkSpawn();
    }

    public void Use()
    {
        disarm.AbilityUsed = true;
    }

    public void Use(Player player, Card card)
    {
        Use();

        DisarmPlayersCardServerRpc(player.NetworkObject, card.NetworkObject);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DisarmPlayersCardServerRpc(NetworkObjectReference playerNetworkObjectReference, NetworkObjectReference cardNetworkObjectReference, ServerRpcParams serverRpcParams = default)
    {
        Player player = Player.GetPlayerFromNetworkReference(playerNetworkObjectReference);

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { player.ClientId.Value }
            }
        };

        DisarmPlayersCardClientRpc(cardNetworkObjectReference, clientRpcParams);
    }

    [ClientRpc]
    private void DisarmPlayersCardClientRpc(NetworkObjectReference cardNetworkObjectReference, ClientRpcParams clientRpcParams = default)
    {
        Card card = Card.GetCardFromNetworkReference(cardNetworkObjectReference);

        card.AbilityUsed = true;
    }
}
