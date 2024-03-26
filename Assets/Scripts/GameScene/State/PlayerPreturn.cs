using System;
using System.Threading.Tasks;

public class PlayerPreturn : State
{
    public static event EventHandler<string[]> OnPlayerPreturn;
    public static event EventHandler OnPlayerPreturnOver;

    public override async Task Start()
    {
        if (Player.LocalInstance.EquippedCards.Count == 0)
        {
            await End();
            return;
        }

        await base.Start();

        OnPlayerPreturn?.Invoke(this, CreateOnPrePlayerTurnMessage());
    }

    public override async Task End()
    {
        StateManager.Instance.GiveCurrentStateToSetNext(StateEnum.PlayerPreturn);

        OnPlayerPreturnOver?.Invoke(this, EventArgs.Empty);

        await Task.CompletedTask;
    }

    private string[] CreateOnPrePlayerTurnMessage()
    {
        return new string[] {
            "YOUR PRETURN",
            $"<color=#{Player.LocalInstance.HexPlayerColor}>{Player.LocalInstance.PlayerName}'s </color>PRETURN"
        };
    }
}

