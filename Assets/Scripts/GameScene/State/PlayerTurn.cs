using System;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerTurn : State
{
    public static event Action OnPlayerTurn;
    public static event Action OnPlayerTurnOver;

    public override async Task Start()
    {
        await base.Start();

        ActionsUI.OnMove += ActionsUI_OnMove;
        ActionsUI.OnAttackCard += ActionsUI_OnAttackCard;

        string[] messages = CreateOnPlayerTurnMessage();

        MessageUI.Instance.SendMessageToEveryoneExceptMe(messages);

        FadeMessageUI.Instance.StartFadeMessage(messages[0]);

        OnPlayerTurn?.Invoke();
    }

    public async Task AttackCard()
    {
        await Awaitable.NextFrameAsync();
    }

    public async Task AttackPlayer()
    {
        await Awaitable.NextFrameAsync();
    }

    public async Task Move(Tile tile)
    {
        Player.LocalInstance.SetPlayersPosition(tile);

        await Awaitable.WaitForSecondsAsync(1);
    }

    public async override Task End()
    {
        OnPlayerTurnOver?.Invoke();

        ActionsUI.OnMove -= ActionsUI_OnMove;
        ActionsUI.OnAttackCard -= ActionsUI_OnAttackCard;

        await base.End();

        StateManager.Instance.GiveCurrentStateToSetNext(StateEnum.PlayerTurn);
    }

    private async void ActionsUI_OnMove(Tile obj)
    {
        await Move(obj);
    }

    private async void ActionsUI_OnAttackCard(Tile tile)
    {
        await AttackCard();
    }

    private string[] CreateOnPlayerTurnMessage()
    {
        return new string[] {
            "YOUR TURN TO PLAY",
            $"<color=#{Player.LocalInstance.HexPlayerColor}>{Player.LocalInstance.PlayerName}'s </color>" + $"TURN TO PLAY."
        };
    }
}

