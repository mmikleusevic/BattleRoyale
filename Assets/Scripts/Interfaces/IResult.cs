using Unity.Netcode;

public interface IResult
{
    void SetResultServerRpc(int result, RollTypeEnum rollTypeEnum, ServerRpcParams serverRpcParams = default);
}