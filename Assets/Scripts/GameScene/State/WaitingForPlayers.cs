using Task = System.Threading.Tasks.Task;

public partial class WaitingForPlayers : State
{
    public override async Task Start()
    {
        await base.Start();

        MessageUI.Instance.SetMessage(CreateOnWaitingForPlayersMessage());
    }

    private string CreateOnWaitingForPlayersMessage()
    {
        return "WAITING FOR PLAYERS";
    }
}

