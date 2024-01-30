using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public static Player LocalInstance { get; private set; }

    [SerializeField] private SetVisual playerVisual;

    NetworkVariable<ulong> clientId;

    public Color playerColor;
    public ulong ClientId { get => clientId.Value; }

    private void Awake()
    {
        clientId = new NetworkVariable<ulong>(NetworkObject.OwnerClientId, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    }

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
    }
}
