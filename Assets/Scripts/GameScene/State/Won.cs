using System;
using System.Threading.Tasks;

public class Won : State
{
    public static event Action<string> OnWon;

    public override async Task Start()
    {
        await base.Start();

        OnWon?.Invoke(CreateOnWonMessage());
    }

    private string CreateOnWonMessage()
    {
        return "YOU WON!";
    }

    public static void ResetStaticData()
    {
        OnWon = null;
    }
}

