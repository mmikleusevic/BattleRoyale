
using Unity.Netcode;

public interface ICardResults
{
    void SetResultServerRpc(int[] results, int result, RollTypeEnum rollTypeEnum, ServerRpcParams serverRpcParams = default);
}