using Unity.Netcode;
using UnityEngine;

public class Dice : NetworkBehaviour
{
    [SerializeField] private SetVisual diceVisual;

    void Start()
    {
        diceVisual.SetColor(Player.LocalInstance.playerColor);
    }

    public override void OnNetworkSpawn()
    {
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
