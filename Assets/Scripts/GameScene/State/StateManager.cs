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

        await this.state.Start();
    }

    public StateEnum GetState()
    {
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

    public void SetStateToClients(StateEnum state, ClientRpcParams clientRpcParams = default)
    {
        SetStateToClientsClientRpc(state, clientRpcParams);
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
