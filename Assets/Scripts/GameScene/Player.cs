using System;
using System.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
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
    public Vector2 GridPosition { get; private set; }
    public int Movement { get; private set; }
    public int ActionPoints { get; private set; }

    public string HexPlayerColor { get; private set; }
    public string PlayerName { get; private set; }

    private int defaultMovement = 0;

    private int defaultActionPoints = 2;

    private float moveSpeed = 20f;

    private void Awake()
    {
        ClientId = new NetworkVariable<ulong>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        IsDead = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        playerParticleSystem = particleCircle.GetComponent<ParticleSystem>();

        Movement = defaultMovement;
        ActionPoints = defaultActionPoints;

        PlayerTurn.OnPlayerTurn += PlayerTurn_OnPlayerTurn;
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LocalInstance = this;
            ClientId.Value = NetworkObject.OwnerClientId;
        }

        InitializePlayerClientRpc();

        base.OnNetworkSpawn();
    }

    public override void OnDestroy()
    {
        PlayerTurn.OnPlayerTurn -= PlayerTurn_OnPlayerTurn;

        base.OnDestroy();
    }

    [ClientRpc]
    private void InitializePlayerClientRpc()
    {
        PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);

        Color color = GameMultiplayer.Instance.GetPlayerColor(playerData.colorId);

        HexPlayerColor = color.ToHexString();
        PlayerName = playerData.playerName.ToString();

        playerVisual.SetColor(color);

        playerAnimator = playerVisual.gameObject.GetComponent<PlayerAnimator>();
    }

    private void PlayerTurn_OnPlayerTurn(object sender, string[] e)
    {
        ResetActionsAndMovement();
    }

    public void SetPlayersPosition(Card card)
    {
        PlayerCardSpot playerCardSpot = card.FindFirstEmptyPlayerSpot();

        Vector3 targetPosition = card.transform.position + playerCardSpot.position;

        StartCoroutine(PlayWalkingAnimation(targetPosition));

        if (Movement > 0)
        {
            SubtractMovement();
        }
        else
        {
            SubtractActionPoints();
        }

        GridPosition = card.GridPosition;
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

    public void SubtractMovement()
    {
        Movement--;
    }

    public void SubtractActionPoints()
    {
        ActionPoints--;
    }

    private void ResetActionsAndMovement()
    {
        ActionPoints = defaultActionPoints;
        Movement = defaultMovement;
    }
}
