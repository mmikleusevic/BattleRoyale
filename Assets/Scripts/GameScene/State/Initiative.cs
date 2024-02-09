using System;
using System.Threading.Tasks;

public partial class Initiative : State
{
    public static event EventHandler<string> OnInitiativeStart;

    public override async Task Start()
    {
        await base.Start();

        string message = "ROLL FOR INITIATIVE";

        OnInitiativeStart?.Invoke(this, message);
    }

    public override async Task End()
    {
        await base.End();
    }
}

