using System;
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

        Type stateType = state.GetType();

        if (stateType == typeof(WaitingForPlayers))
        {
            return StateEnum.WaitingForPlayers;
        }
        else if (stateType == typeof(Initiative))
        {
            return StateEnum.Initiative;
        }
        else if (stateType == typeof(PlaceOnGrid))
        {
            return StateEnum.PlaceOnGrid;
        }
        else if (stateType == typeof(PlayerPreturn))
        {
            return StateEnum.PlayerPreturn;
        }
        else if (stateType == typeof(PlayerTurn))
        {
            return StateEnum.PlayerTurn;
        }
        else if (stateType == typeof(EnemyTurn))
        {
            return StateEnum.EnemyTurn;
        }
        else if (stateType == typeof(Lost))
        {
            return StateEnum.Lost;
        }
        else
        {
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
                SetState(StateEnum.EnemyTurn);
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

        ulong[] clientIds = new ulong[PlayerManager.Instance.Players.Count - 1];

        int i = 0;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
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
