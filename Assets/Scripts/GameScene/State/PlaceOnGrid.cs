using System;
using System.Collections;

public class PlaceOnGrid : State
{
    public static event EventHandler<string> OnPlaceOnGrid;
    public override IEnumerator Start()
    {
        string message = "YOUR TURN TO CHOOSE PLACEMENT";
        OnPlaceOnGrid?.Invoke(this, message);

        yield break;
    }
}