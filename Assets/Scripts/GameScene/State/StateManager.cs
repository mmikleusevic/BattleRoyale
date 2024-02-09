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
            case StateEnum.Lost:
                this.state = new Lost();
                break;
            case StateEnum.PlayerTurn:
                this.state = new PlayerTurn();
                break;
            case StateEnum.EnemyTurn:
                this.state = new EnemyTurn();
                break;
            case StateEnum.Won:
                this.state = new Won();
                break;
        }

        await this.state.Start();
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
    PlayerTurn,
    EnemyTurn,
    Lost,
    Won
}
