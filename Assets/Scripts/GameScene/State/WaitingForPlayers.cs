using System;
using Task = System.Threading.Tasks.Task;

public partial class WaitingForPlayers : State
{
    public static event EventHandler<string> OnWaitingForPlayers;

    public override async Task Start()
    {
        await base.Start();

        OnWaitingForPlayers?.Invoke(this, CreateOnWaitingForPlayersMessage());
    }

    private string CreateOnWaitingForPlayersMessage()
    {
        return "WAITING FOR PLAYERS";
    }
}

