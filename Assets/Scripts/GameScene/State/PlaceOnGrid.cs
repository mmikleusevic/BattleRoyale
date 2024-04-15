using System.Threading.Tasks;

public class PlaceOnGrid : State
{
    public override async Task Start()
    {
        await base.Start();

        ActionsUI.OnMove += ActionsUI_OnMove;

        string[] messages = CreateOnPlaceOnGridStartedMessage();

        MessageUI.Instance.SendMessageToEveryoneExceptMe(messages);

        FadeMessageUI.Instance.StartFadeMessage(messages[0]);

        GridManager.Instance.HighlightAllUnoccupiedCards();
    }

    public override async Task End()
    {
        StateManager.Instance.GiveCurrentStateToSetNext(StateEnum.PlaceOnGrid);

        await base.End();
    }

    private async void ActionsUI_OnMove(Tile tile)
    {
        ActionsUI.OnMove -= ActionsUI_OnMove;

        tile.SetIsOccupiedOnPlacingServerRpc();

        Player.LocalInstance.SetPlayersPosition(tile);

        MessageUI.Instance.SendMessageToEveryoneExceptMe(CreateOnPlayerPlacedMessage(tile));

        await End();
    }

    private string[] CreateOnPlaceOnGridStartedMessage()
    {
        return new string[] {
            "YOUR TURN TO CHOOSE PLACEMENT",
            $"<color=#{Player.LocalInstance.HexPlayerColor}>{Player.LocalInstance.PlayerName}'s </color>" + $"TURN TO PLACE."
        };
    }

    private string[] CreateOnPlayerPlacedMessage(Tile tile)
    {
        return new string[] {
            $"YOU PLACED ON {tile.GetCardOrTileName()}",
            $"<color=#{Player.LocalInstance.HexPlayerColor}>{Player.LocalInstance.PlayerName} </color>" + $"placed on {tile.GetCardOrTileName()}"
        };
    }

    public override void Dispose()
    {
        ActionsUI.OnMove -= ActionsUI_OnMove;
    }
}