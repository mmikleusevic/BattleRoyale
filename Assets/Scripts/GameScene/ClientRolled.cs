using System;
using Unity.Netcode;

public struct ClientRolled : IEquatable<ClientRolled>, INetworkSerializable
{ 
    public ulong clientId;
    public bool rolled;

    public bool Equals(ClientRolled other)
    {
        return
            clientId == other.clientId &&
            rolled == other.rolled;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref rolled);
    }   
}
