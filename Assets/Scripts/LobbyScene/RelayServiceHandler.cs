using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayServiceHandler
{
    public async Task<Allocation> AllocateRelay()
    {
        try
        {
            return await RelayService.Instance.CreateAllocationAsync(GameMultiplayer.MAX_PLAYER_AMOUNT - 1);
        }
        catch (RelayServiceException ex)
        {
            Debug.LogError(ex.Message);

            return default;
        }
    }

    public async Task<string> GetRelayJoinCode(Allocation allocation)
    {
        try
        {
            return await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        }
        catch (RelayServiceException ex)
        {
            Debug.LogError(ex.Message);

            return default;
        }
    }

    public async Task<JoinAllocation> JoinRelay(string joinCode)
    {
        try
        {
            return await RelayService.Instance.JoinAllocationAsync(joinCode);
        }
        catch (RelayServiceException ex)
        {
            Debug.LogError(ex.Message);

            return default;
        }
    }

    public async Task LobbyJoinRelayStartClient(Lobby joinedLobby)
    {
        try
        {
            string relayJoinCode = joinedLobby.Data[GameLobby.KEY_RELAY_JOIN_CODE].Value;

            JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

            GameMultiplayer.Instance.StartClient();
        }
        catch (RelayServiceException ex)
        {
            Debug.LogError(ex.Message);
            throw;
        }
    }
}