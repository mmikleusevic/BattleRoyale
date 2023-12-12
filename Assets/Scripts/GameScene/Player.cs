using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    private bool isDead = false;

    public static Player LocalInstance { get; private set; }

    [SerializeField] private SetVisual playerVisual;

    public Color playerColor;

    private void Start()
    {
        PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        playerColor = GameMultiplayer.Instance.GetPlayerColor(playerData.colorId);

        playerVisual.SetColor(playerColor);
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
