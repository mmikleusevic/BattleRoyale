using System;
using System.Threading.Tasks;

public partial class Initiative : State
{
    public static event EventHandler<string> OnInitiativeStart;

    public override async Task Start()
    {
        await base.Start();

        OnInitiativeStart?.Invoke(this, CreateOnInitiativeMessage());
    }

    public override async Task End()
    {
        await base.End();
    }

    private string CreateOnInitiativeMessage()
    {
        return "ROLL FOR INITIATIVE";
    }
}

