using System;
using UnityEngine;
using UnityEngine.UI;

public class EndTurnUI : MonoBehaviour
{
    public static event Action<string[]> OnEndTurn;

    [SerializeField] private Button endTurnButton;
    [SerializeField] private ParticleSystem particleGlow;

    private void Awake()
    {
        endTurnButton.onClick.AddListener(async () =>
        {
            Hide();
            OnEndTurn?.Invoke(SendToMessageUI());
            particleGlow.Stop();
            await StateManager.Instance.EndState();
        });

        PlayerTurn.OnPlayerTurn += PlayerTurn_OnPlayerTurn;
        PlayerBattleResults.OnPlayerBattleRollOver += PlayerBattleResults_OnPlayerBattleRollOver;
        PlayerBattleResults.OnAfterBattleResolved += PlayerBattleResults_OnAfterBattleResolved;
        Player.OnNoMoreMovementOrActionPoints += Player_OnNoMoreMovementOrActionPoints;

        particleGlow.Stop();

        Hide();
    }

    public void OnDestroy()
    {
        PlayerTurn.OnPlayerTurn -= PlayerTurn_OnPlayerTurn;
        PlayerBattleResults.OnPlayerBattleRollOver -= PlayerBattleResults_OnPlayerBattleRollOver;
        PlayerBattleResults.OnAfterBattleResolved -= PlayerBattleResults_OnAfterBattleResolved;
        Player.OnNoMoreMovementOrActionPoints -= Player_OnNoMoreMovementOrActionPoints;

        endTurnButton.onClick.RemoveAllListeners();
    }

    private void PlayerTurn_OnPlayerTurn(object sender, string[] e)
    {
        Show();
    }

    private void PlayerBattleResults_OnPlayerBattleRollOver(string obj)
    {
        Hide();
    }

    private void PlayerBattleResults_OnAfterBattleResolved()
    {
        Show();
    }

    private void Player_OnNoMoreMovementOrActionPoints()
    {
        particleGlow.Play();
    }

    private string[] SendToMessageUI()
    {
        return new string[]
        {
            $"YOU ENDED YOUR TURN",
            $"<color=#{Player.LocalInstance.HexPlayerColor}>{Player.LocalInstance.PlayerName}</color> has ended his turn."
        };
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
