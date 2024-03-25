using TMPro;
using UnityEngine;

public class PCInfoUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI movementsText;
    [SerializeField] private TextMeshProUGUI actionsText;
    [SerializeField] private TextMeshProUGUI pointsText;
    [SerializeField] private TextMeshProUGUI deadText;
    [SerializeField] private TextMeshProUGUI sipCounterText;
    [SerializeField] private TextMeshProUGUI currentActivePlayerText;

    private void Awake()
    {
        Initiative.OnInitiativeStart += Initiative_OnInitiativeStart;
        Player.OnPlayerTurnSet += Player_OnPlayerTurnSet;
        Player.OnPlayerMoved += Player_OnPlayerMoved;
        Player.OnPlayerActionUsed += Player_OnPlayerActionUsed;
        Player.OnPlayerDiedCardBattle += Player_OnPlayerDiedCardBattle;
        Player.OnPlayerResurrected += Player_OnPlayerResurrected;
        Player.OnPlayerPointsChanged += Player_OnPlayerPointsChanged;
        Player.OnPlayerDiedPlayerBattle += Player_OnPlayerDiedPlayerBattle;
        Player.OnPlayerSipCounterChanged += Player_OnPlayerSipCounterChanged;

        Hide();
    }

    private void Start()
    {
        PlayerManager.Instance.OnActivePlayerChanged += Instance_OnActivePlayerChanged;
    }

    private void OnDestroy()
    {
        Initiative.OnInitiativeStart -= Initiative_OnInitiativeStart;
        Player.OnPlayerTurnSet -= Player_OnPlayerTurnSet;
        Player.OnPlayerMoved -= Player_OnPlayerMoved;
        Player.OnPlayerActionUsed -= Player_OnPlayerActionUsed;
        Player.OnPlayerDiedCardBattle -= Player_OnPlayerDiedCardBattle;
        Player.OnPlayerResurrected -= Player_OnPlayerResurrected;
        Player.OnPlayerPointsChanged -= Player_OnPlayerPointsChanged;
        Player.OnPlayerDiedPlayerBattle -= Player_OnPlayerDiedPlayerBattle;
        PlayerManager.Instance.OnActivePlayerChanged -= Instance_OnActivePlayerChanged;
        Player.OnPlayerSipCounterChanged -= Player_OnPlayerSipCounterChanged;
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
    }

    private void Player_OnPlayerMoved(object sender, string[] e)
    {
        SetMovementsText();
        SetActionsText();
    }

    private void Player_OnPlayerActionUsed()
    {
        SetActionsText();
    }

    private void Player_OnPlayerDiedCardBattle()
    {
        SetIsDeadText();
    }

    private void Player_OnPlayerResurrected(string[] obj)
    {
        SetIsDeadText();
    }

    private void Player_OnPlayerPointsChanged()
    {
        SetPointsText();
    }

    private void Player_OnPlayerDiedPlayerBattle(string[] messages)
    {
        SetIsDeadText();
    }

    private void Instance_OnActivePlayerChanged(Player obj)
    {
        SetActivePlayerText(obj);
    }

    private void Player_OnPlayerSipCounterChanged()
    {
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
        pointsText.text = $"Points: {Player.LocalInstance.Points.Value}";
    }

    private void SetPlayerNameText()
    {
        playerNameText.text = "Player Name: ";
        playerNameText.text += $"<color=#{Player.LocalInstance.HexPlayerColor}>{Player.LocalInstance.PlayerName}</color>";
    }

    private void SetActivePlayerText(Player player)
    {
        currentActivePlayerText.text = "Active player: ";
        currentActivePlayerText.text += $"<color=#{player.HexPlayerColor}>{player.PlayerName}</color>";
    }

    private void SetIsDeadText()
    {
        if (Player.LocalInstance.IsDead.Value)
        {
            deadText.text = "Dead: <color=red>Yes</color>";
        }
        else
        {
            deadText.text = "Dead: <color=green>No</color>";
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
