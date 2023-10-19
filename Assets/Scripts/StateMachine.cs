using UnityEngine;

public abstract class StateMachine : MonoBehaviour
{
    protected State State;

    public async void SetState(State state)
    {
        State = state;

        await State.Start();
    }
}
