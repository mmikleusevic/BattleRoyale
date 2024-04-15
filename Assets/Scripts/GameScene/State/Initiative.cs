using System;
using System.Threading.Tasks;

public partial class Initiative : State
{
    public static event Action OnInitiativeStart;

    public override async Task Start()
    {
        await base.Start();

        string message = CreateOnInitiativeMessage();

        MessageUI.Instance.SetMessage(message);
        FadeMessageUI.Instance.StartFadeMessage(message);

        OnInitiativeStart?.Invoke();
    }

    private string CreateOnInitiativeMessage()
    {
        return "ROLL FOR INITIATIVE";
    }

    public static void ResetStaticData()
    {
        OnInitiativeStart = null;
    }
}

