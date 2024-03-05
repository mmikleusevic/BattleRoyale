using TMPro;
using UnityEngine;

public class PlayerInfoUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI movementsText;
    [SerializeField] private TextMeshProUGUI actionsText;
    [SerializeField] private TextMeshProUGUI pointsText;
    [SerializeField] private TextMeshProUGUI deadText;
    [SerializeField] private TextMeshProUGUI sipCounterText;

    private void Awake()
    {
        Initiative.OnInitiativeStart += Initiative_OnInitiativeStart;
        Player.OnPlayerTurnSet += Player_OnPlayerTurnSet;
        Player.OnPlayerMoved += Player_OnPlayerMoved;
        Player.OnPlayerActionUsed += Player_OnPlayerActionUsed;
        Player.OnPlayerDiedCardBattle += Player_OnPlayerDiedCardBattle;
        Player.OnPlayerResurrected += Player_OnPlayerResurrected;

        Hide();
    }

    private void OnDestroy()
    {
        Initiative.OnInitiativeStart -= Initiative_OnInitiativeStart;
        Player.OnPlayerTurnSet -= Player_OnPlayerTurnSet;
        Player.OnPlayerMoved -= Player_OnPlayerMoved;
        Player.OnPlayerActionUsed -= Player_OnPlayerActionUsed;
        Player.OnPlayerDiedCardBattle -= Player_OnPlayerDiedCardBattle;
        Player.OnPlayerResurrected -= Player_OnPlayerResurrected;
    }

    private void Initiative_OnInitiativeStart(object sender, string e)
    {
        Show();

        SetPlayerNameText();
        SetPointsText();
        SetMovementsText();
        SetActionsText();
        SetIsDeadText();
    }

    private void Player_OnPlayerTurnSet()
    {
        SetMovementsText();
        SetActionsText();
        SetIsDeadText();
        SetSipCounter();
    }

    private void Player_OnPlayerMoved(object sender, string e)
    {
        SetMovementsText();
        SetActionsText();
        SetIsDeadText();
    }

    private void Player_OnPlayerActionUsed()
    {
        SetActionsText();
        SetIsDeadText();
    }

    private void Player_OnPlayerDiedCardBattle()
    {
        SetIsDeadText();
    }

    private void Player_OnPlayerResurrected(string[] obj)
    {
        SetActionsText();
        SetIsDeadText();
        SetSipCounter();
    }

    private void SetMovementsText()
    {
        movementsText.text = $"Movements: {Player.LocalInstance.Movement}";
    }

    private void SetActionsText()
    {
        actionsText.text = $"Actions: {Player.LocalInstance.ActionPoints}";
    }

    private void SetPointsText()
    {
        pointsText.text = $"Points: {Player.LocalInstance.Points}";
    }

    private void SetPlayerNameText()
    {
        playerNameText.text = "Player Name: ";
        playerNameText.text += $"<color=#{Player.LocalInstance.HexPlayerColor}>{Player.LocalInstance.PlayerName}</color>";
    }

    private void SetIsDeadText()
    {
        if (Player.LocalInstance.IsDead.Value)
        {
            deadText.text = "Dead: Yes";
        }
        else
        {
            deadText.text = "Dead: No";
        }
    }

    private void SetSipCounter()
    {
        sipCounterText.text = $"Number of sips: {Player.LocalInstance.SipCounter}";
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
