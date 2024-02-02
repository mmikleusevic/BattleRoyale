using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public static Player LocalInstance { get; private set; }

    [SerializeField] private SetVisual playerVisual;

    public Color playerColor;
    public NetworkVariable<ulong> ClientId { get; private set; }

    private void Awake()
    {
        ClientId = new NetworkVariable<ulong>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        if (IsOwner)
        {
            LocalInstance = this;
        }
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
            ClientId.Value = NetworkObject.OwnerClientId;
        }

        base.OnNetworkSpawn();
    }
}
