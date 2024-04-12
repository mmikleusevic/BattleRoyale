using System;
using System.Threading.Tasks;

public class PlayerPreturn : State
{
    public static event Action OnPlayerPreturn;
    public static event Action OnPlayerPreturnOver;

    public override async Task Start()
    {
        if (Player.LocalInstance.EquippedCards.Count == 0)
        {
            await End();
            return;
        }

        await base.Start();

        string[] messages = CreateOnPrePlayerTurnMessage();

        MessageUI.Instance.SendMessageToEveryoneExceptMe(messages);

        FadeMessageUI.Instance.StartFadeMessage(messages[0]);

        OnPlayerPreturn?.Invoke();
    }

    public override async Task End()
    {
        StateManager.Instance.GiveCurrentStateToSetNext(StateEnum.PlayerPreturn);

        OnPlayerPreturnOver?.Invoke();

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

