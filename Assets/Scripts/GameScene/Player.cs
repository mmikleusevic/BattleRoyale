using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public static Player LocalInstance { get; private set; }

    [SerializeField] private List<Vector3> spawnPositionList = new List<Vector3>();

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LocalInstance = this;
        }

        int playerIndex = GameMultiplayer.Instance.GetPlayerDataIndexFromClientId(OwnerClientId);

        transform.position = spawnPositionList[playerIndex];

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += GameManager_OnClientDisconnectCallback;
        }

        //TODO fix player names sync
        //TODO fix networkspawn before everyone loaded
        GameMultiplayer.Instance.SetNameClientRpc(gameObject, "Player" + playerIndex);
    }

    private void GameManager_OnClientDisconnectCallback(ulong obj)
    {
        //Destroy players objects
        Destroy(gameObject);
    }
}
