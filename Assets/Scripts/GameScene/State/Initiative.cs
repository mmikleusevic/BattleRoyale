using System;
using System.Threading.Tasks;

public partial class Initiative : State
{
    public static event EventHandler<string> OnInitiativeStart;

    public override async Task Start()
    {
        await base.Start();

        Card.OnCardPressed += Card_OnCardPressed;

        string message = "ROLL FOR INITIATIVE";

        OnInitiativeStart?.Invoke(this, message);
    }

    private async void Card_OnCardPressed(object sender, EventArgs e)
    {
        Card card = (Card)sender; 

        GridManager.Instance.PlacePlayerOnGrid(card);

        await End();
    }

    public override async Task End()
    {
        await base.End();

        Card.OnCardPressed -= Card_OnCardPressed;

        GameManager.Instance.NextClientInitiativeServerRpc();
    }
}

