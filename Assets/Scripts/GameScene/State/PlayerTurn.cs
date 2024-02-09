using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerTurn : State
{
    public static event EventHandler<string[]> OnPlayerTurn;
    public static event EventHandler<string> OnPlayerMoved;

    public override async Task Start()
    {
        await base.Start();

        ActionsUI.OnMove += ActionsUI_OnMove;

        OnPlayerTurn?.Invoke(this, CreateOnPlayerTurnMessage());
    }

    public IEnumerator AttackCard()
    {
        yield break;
    }

    public IEnumerator AttackPlayer()
    {
        yield break;
    }

    public async Task Move(Card card)
    {
        Player.LocalInstance.SetPlayersPosition(card);

        string message = CreateOnPlayerMovedMessage(card);

        OnPlayerMoved?.Invoke(this, message);

        await Awaitable.WaitForSecondsAsync(1);
    }

    public async override Task End()
    {
        await base.End();

        ActionsUI.OnMove -= ActionsUI_OnMove;

        StateManager.Instance.SetState(StateEnum.EnemyTurn);
        StateManager.Instance.NextClientStateServerRpc(StateEnum.PlayerTurn);
    }

    private async void ActionsUI_OnMove(Card obj)
    {
        await Move(obj);
    }

    private string CreateOnPlayerMovedMessage(Card card)
    {
        return $"<color=#{Player.LocalInstance.HexPlayerColor}>{Player.LocalInstance.PlayerName} </color>" + $"moved to {card.Name}";
    }

    private string[] CreateOnPlayerTurnMessage()
    {
        return new string[] {
            "YOUR TURN TO PLAY",
            $"<color=#{Player.LocalInstance.HexPlayerColor}>{Player.LocalInstance.PlayerName}'s </color>" + $"TURN TO PLAY."
        };
    }
}

