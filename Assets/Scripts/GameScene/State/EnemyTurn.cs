using System.Threading.Tasks;

public class EnemyTurn : State
{
    public override async Task Start()
    {
        await base.Start();

        ActionsUI.OnMove += ActionsUI_OnMove;
    }

    public void Move(Tile tile)
    {
        Player.LocalInstance.SetPlayersPosition(tile);
    }

    private void ActionsUI_OnMove(Tile obj)
    {
        Move(obj);
    }

    public async override Task End()
    {
        ActionsUI.OnMove -= ActionsUI_OnMove;

        await base.End();
    }
}