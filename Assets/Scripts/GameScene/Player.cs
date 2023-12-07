using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    private bool isDead = false;

    public static Player LocalInstance { get; private set; }

    [SerializeField] private PlayerVisual playerVisual;

    private void Start()
    {
        PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        playerVisual.SetPlayerColor(GameMultiplayer.Instance.GetPlayerColor(playerData.colorId));
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LocalInstance = this;
        }      

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += GameManager_OnClientDisconnectCallback;
        }
    }

    private void GameManager_OnClientDisconnectCallback(ulong obj)
    {
        Destroy(gameObject);
    }
}
