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

    public void Use(Card card)
    {
        Use();

        DisarmPlayersCardServerRpc(card.NetworkObject);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DisarmPlayersCardServerRpc(NetworkObjectReference cardNetworkObjectReference, ServerRpcParams serverRpcParams = default)
    {
        DisarmPlayersCardClientRpc(cardNetworkObjectReference);
    }

    [ClientRpc]
    private void DisarmPlayersCardClientRpc(NetworkObjectReference cardNetworkObjectReference, ClientRpcParams clientRpcParams = default)
    {
        Card card = Card.GetCardFromNetworkReference(cardNetworkObjectReference);

        card.AbilityUsed = true;
    }
}
