using System;
using System.Threading.Tasks;

public class PlaceOnGrid : State
{
    public static event EventHandler<string> OnPlaceOnGrid;
    public override async Task Start()
    {
        await base.Start();

        string message = "YOUR TURN TO CHOOSE PLACEMENT";

        OnPlaceOnGrid?.Invoke(this, message);
    }
}