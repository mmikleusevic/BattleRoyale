using System;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerTurn : State
{
    public static event EventHandler<string[]> OnPlayerTurn;

    public override async Task Start()
    {
        await base.Start();

        ActionsUI.OnMove += ActionsUI_OnMove;
        ActionsUI.OnAttackCard += ActionsUI_OnAttackCard;

        OnPlayerTurn?.Invoke(this, CreateOnPlayerTurnMessage());
    }

    public async Task AttackCard()
    {
        await Awaitable.NextFrameAsync();
    }

    public async Task AttackPlayer()
    {
        await Awaitable.NextFrameAsync();
    }

    public async Task Move(Card card)
    {
        Player.LocalInstance.SetPlayersPosition(card);

        await Awaitable.WaitForSecondsAsync(1);
    }

    public async override Task End()
    {
        await base.End();

        ActionsUI.OnMove -= ActionsUI_OnMove;
        ActionsUI.OnAttackCard -= ActionsUI_OnAttackCard;

        StateManager.Instance.SetState(StateEnum.EnemyTurn);
        StateManager.Instance.NextClientStateServerRpc(StateEnum.PlayerTurn);
    }

    private async void ActionsUI_OnMove(Card obj)
    {
        await Move(obj);
    }

    private async void ActionsUI_OnAttackCard(Card card)
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

