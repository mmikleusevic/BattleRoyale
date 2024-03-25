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
            ParticleSystemManager.Instance.Stop(particleGlow);
            await StateManager.Instance.EndState();
        });

        PlayerTurn.OnPlayerTurn += PlayerTurn_OnPlayerTurn;
        PlayerBattleResults.OnPlayerBattleRollOver += PlayerBattleResults_OnPlayerBattleRollOver;
        PlayerBattleResults.OnAfterBattleResolved += PlayerBattleResults_OnAfterBattleResolved;
        Player.OnNoMoreMovementOrActionPoints += Player_OnNoMoreMovementOrActionPoints;

        ParticleSystemManager.Instance.Stop(particleGlow);

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
        endTurnButton.interactable = true;

        Show();
    }

    private void PlayerBattleResults_OnPlayerBattleRollOver(string obj)
    {
        endTurnButton.interactable = false;
    }

    private void PlayerBattleResults_OnAfterBattleResolved()
    {
        endTurnButton.interactable = true;
    }

    private void Player_OnNoMoreMovementOrActionPoints()
    {
        ParticleSystemManager.Instance.Play(particleGlow);
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
