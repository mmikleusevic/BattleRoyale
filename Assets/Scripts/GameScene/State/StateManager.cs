using UnityEngine;

public class StateManager : MonoBehaviour, IStateManager
{
    protected State state;

    public void SetState(StateEnum state)
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
            case StateEnum.Move:
                this.state = new Move();
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
            case StateEnum.GameFinished:
                this.state = new GameFinished();
                break;
        }

        StartCoroutine(this.state.Start());
    }
}

public enum StateEnum
{
    WaitingForPlayers,
    Initiative,
    PlaceOnGrid,
    Lost,
    Move,
    PlayerTurn,
    EnemyTurn,
    Won,
    GameFinished
}
