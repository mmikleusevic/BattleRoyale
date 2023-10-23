using UnityEngine;

public abstract class StateMachine : MonoBehaviour
{
    protected State state;

    public async void SetState(State state)
    {
        this.state = state;

        await this.state.Start();
    }
}
