using System;
using System.Threading.Tasks;

public class PlaceOnGrid : State
{
    public static event EventHandler<string[]> OnPlaceOnGrid;
    public static event EventHandler<string[]> OnPlayerPlaced;
    public override async Task Start()
    {
        await base.Start();

        ActionsUI.OnMove += ActionsUI_OnMove;

        OnPlaceOnGrid?.Invoke(this, CreateOnPlaceOnGridStartedMessage());
    }

    public override async Task End()
    {
        if (PlayerManager.Instance.ActivePlayer == PlayerManager.Instance.LastPlayer)
        {
            StateManager.Instance.NextClientStateServerRpc(StateEnum.PlayerTurn);
        }
        else
        {
            StateManager.Instance.NextClientStateServerRpc(StateEnum.PlaceOnGrid);
        }

        await base.End();
    }

    private async void ActionsUI_OnMove(Tile tile)
    {
        ActionsUI.OnMove -= ActionsUI_OnMove;

        tile.SetIsOccupiedOnPlacingServerRpc();

        Player.LocalInstance.SetPlayersPosition(tile);

        OnPlayerPlaced?.Invoke(this, CreateOnPlayerPlacedMessage(tile));

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
}