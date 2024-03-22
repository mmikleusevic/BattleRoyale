using System;
using System.Threading.Tasks;

public class Lost : State
{
    public static event Action<string> OnLost;

    public override async Task Start()
    {
        await base.Start();

        OnLost?.Invoke(CreateOnLostMessage());
    }

    private string CreateOnLostMessage()
    {
        return "YOU LOST!";
    }
}