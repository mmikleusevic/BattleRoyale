using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public static Player LocalInstance { get; private set; }

    [SerializeField] private SetVisual playerVisual;
    [SerializeField] private GameObject particleCircle;

    ParticleSystem playerParticleSystem;
    PlayerAnimator playerAnimator;
    public NetworkVariable<ulong> ClientId { get; private set; }
    public NetworkVariable<bool> IsDead { get; set; }

    public Color playerColor;

    private int defaultMovement = 0;
    private int movement;
    private int defaultActionPoints = 2;
    private int actionPoints;

    private float moveSpeed = 15f;

    private void Awake()
    {
        ClientId = new NetworkVariable<ulong>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        IsDead = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        playerParticleSystem = particleCircle.GetComponent<ParticleSystem>();
        movement = defaultMovement;
        actionPoints = defaultActionPoints;
    }

    private void Start()
    {
        PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        playerColor = GameMultiplayer.Instance.GetPlayerColor(playerData.colorId);

        playerVisual.SetColor(playerColor);

        playerAnimator = playerVisual.gameObject.GetComponent<PlayerAnimator>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LocalInstance = this;
            ClientId.Value = NetworkObject.OwnerClientId;
        }

        base.OnNetworkSpawn();
    }

    public void SetPlayersPosition(Card card, PlayerCardSpot playerCardSpot)
    {
        Vector3 targetPosition = card.transform.position + playerCardSpot.position;

        StartCoroutine(PlayWalkingAnimation(targetPosition));
    }

    private IEnumerator PlayWalkingAnimation(Vector3 targetPosition)
    {
        MoveServerRpc();

        while (targetPosition != transform.position)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            yield return null;
        }

        StopMovingServerRpc();

        yield return null;
    }

    [ServerRpc(RequireOwnership = false)]
    private void MoveServerRpc(ServerRpcParams serverRpcParams = default)
    {
        MoveClientRpc();
    }

    [ClientRpc]
    private void MoveClientRpc(ClientRpcParams clientRpcParams = default)
    {
        Move();
    }

    private void Move()
    {
        playerAnimator.MoveAnimation();
    }

    [ServerRpc(RequireOwnership = false)]
    private void StopMovingServerRpc()
    {
        StopMovingClientRpc();
    }

    [ClientRpc]
    private void StopMovingClientRpc()
    {
        StopMoving();
    }

    private void StopMoving()
    {
        playerAnimator.StopMovingAnimation();
    }

    public void ShowParticleCircle()
    {
        particleCircle.SetActive(true);
        playerParticleSystem.Play();
    }

    public void HideParticleCircle()
    {
        particleCircle.SetActive(false);
        playerParticleSystem.Stop();
    }

    private void ResetActionsAndMovement()
    {
        actionPoints = defaultActionPoints;
        movement = defaultMovement;
    }
}
