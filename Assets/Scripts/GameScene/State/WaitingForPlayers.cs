using System;
using System.Threading.Tasks;

public partial class WaitingForPlayers : State
{
    public static event EventHandler<string> OnWaitingForPlayers;

    public override async Task Start()
    {
        await base.Start();

        string message = "WAITING FOR PLAYERS";

        OnWaitingForPlayers?.Invoke(this, message);
    }
}

