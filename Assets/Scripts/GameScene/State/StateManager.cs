using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;

public class StateManager : NetworkBehaviour, IStateManager
{
    public static StateManager Instance { get; private set; }

    protected State state;

    private void Awake()
    {
        Instance = this;
    }

    public async void SetState(StateEnum state)
    {
        if (GetState() == StateEnum.EnemyTurn) await this.state.End();

        switch (state)
        {
            case StateEnum.WaitingForPlayers:
                this.state = new WaitingForPlayers();
                break;
            case StateEnum.Initiative:
                this.state = new Initiative();
                break;
            case StateEnum.PlaceOnGrid:
                this.state = new PlaceOnGrid();
                break;
            case StateEnum.PlayerPreturn:
                this.state = new PlayerPreturn();
                break;
            case StateEnum.PlayerTurn:
                this.state = new PlayerTurn();
                break;
            case StateEnum.EnemyTurn:
                this.state = new EnemyTurn();
                break;
            case StateEnum.Lost:
                this.state = new Lost();
                break;
            case StateEnum.Won:
                this.state = new Won();
                break;
        }

        if (Player.LocalInstance)
        {
            Player.LocalInstance.UpdateCurrentState(state);
        }

        await this.state.Start();
    }

    public StateEnum GetState()
    {
        if (state == null) return StateEnum.WaitingForPlayers;

        switch (state)
        {
            case WaitingForPlayers:
                return StateEnum.WaitingForPlayers;
            case Initiative:
                return StateEnum.Initiative;
            case PlaceOnGrid:
                return StateEnum.PlaceOnGrid;
            case PlayerPreturn:
                return StateEnum.PlayerPreturn;
            case PlayerTurn:
                return StateEnum.PlayerTurn;
            case EnemyTurn:
                return StateEnum.EnemyTurn;
            case Lost:
                return StateEnum.Lost;
            default:
                return StateEnum.Won;
        }
    }

    public async Task EndState()
    {
        await state.End();
    }

    public void GiveCurrentStateToSetNext(StateEnum currentState)
    {
        switch (currentState)
        {
            case StateEnum.WaitingForPlayers:
                SetStateToClients(StateEnum.Initiative);
                break;
            case StateEnum.Initiative:
                NextClientStateServerRpc(StateEnum.PlaceOnGrid);
                break;
            case StateEnum.PlaceOnGrid:
                if (PlayerManager.Instance.ActivePlayer == PlayerManager.Instance.LastPlayer)
                {
                    SetEnemyStateToEveryoneExceptNextPlayer();
                }
                else
                {
                    NextClientStateServerRpc(StateEnum.PlaceOnGrid);
                }
                break;
            case StateEnum.PlayerPreturn:
                SetState(StateEnum.PlayerTurn);
                break;
            case StateEnum.PlayerTurn:
                if (PlayerManager.Instance.ActivePlayers.Count > 1)
                {
                    SetState(StateEnum.EnemyTurn);
                }

                NextClientStateServerRpc(StateEnum.PlayerPreturn);
                break;
            case StateEnum.EnemyTurn:
                break;
            case StateEnum.Lost:
                break;
            case StateEnum.Won:
                break;
        }
    }

    public void SetStateToClients(StateEnum state, ClientRpcParams clientRpcParams = default)
    {
        SetStateToClientsClientRpc(state, clientRpcParams);
    }

    public void SetEnemyStateToEveryoneExceptNextPlayer()
    {
        NextClientStateServerRpc(StateEnum.PlayerPreturn);

        SetEnemyStateToEveryoneExceptNextPlayerServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetEnemyStateToEveryoneExceptNextPlayerServerRpc(ServerRpcParams serverRpcParams = default)
    {
        Player activePlayer = PlayerManager.Instance.ActivePlayer;

        ulong[] clientIds = new ulong[PlayerManager.Instance.ActivePlayers.Count - 1];

        int i = 0;
        foreach (ulong clientId in PlayerManager.Instance.ActivePlayers.Select(a => a.ClientId.Value))
        {
            if (clientId == activePlayer.ClientId.Value) continue;

            clientIds[i] = clientId;

            i++;
        }

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = clientIds
            }
        };

        SetStateToClientsClientRpc(StateEnum.EnemyTurn, clientRpcParams);
    }

    [ClientRpc]
    private void SetStateToClientsClientRpc(StateEnum state, ClientRpcParams clientRpcParams = default)
    {
        SetState(state);
    }

    [ServerRpc(RequireOwnership = false)]
    public void NextClientStateServerRpc(StateEnum state)
    {
        SetStateForNextPlayer(state);
    }

    private void SetStateForNextPlayer(StateEnum state)
    {
        PlayerManager.Instance.SetNextActivePlayerClientRpc();

        Player activePlayer = PlayerManager.Instance.ActivePlayer;

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { activePlayer.ClientId.Value }
            }
        };

        SetStateToClients(state, clientRpcParams);
    }
}

public enum StateEnum
{
    WaitingForPlayers,
    Initiative,
    PlaceOnGrid,
    PlayerPreturn,
    PlayerTurn,
    EnemyTurn,
    Lost,
    Won
}
