using TMPro;
using UnityEngine;

public class PCInfoUI : MonoBehaviour
{
    public static PCInfoUI Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI movementsText;
    [SerializeField] private TextMeshProUGUI actionsText;
    [SerializeField] private TextMeshProUGUI pointsText;
    [SerializeField] private TextMeshProUGUI deadText;
    [SerializeField] private TextMeshProUGUI sipCounterText;
    [SerializeField] private TextMeshProUGUI currentActivePlayerText;

    private void Awake()
    {
        Instance = this;

        Initiative.OnInitiativeStart += Initiative_OnInitiativeStart;
        Player.OnPlayerTurnSet += Player_OnPlayerTurnSet;
        Player.OnPlayerMoved += Player_OnPlayerMoved;

        Hide();
    }

    private void OnDestroy()
    {
        Initiative.OnInitiativeStart -= Initiative_OnInitiativeStart;
        Player.OnPlayerTurnSet -= Player_OnPlayerTurnSet;
        Player.OnPlayerMoved -= Player_OnPlayerMoved;
    }

    private void Initiative_OnInitiativeStart()
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

    private void Player_OnPlayerMoved()
    {
        SetMovementsText();
        SetActionsText();
    }

    public void SetMovementsText()
    {
        movementsText.text = $"Movements: {Player.LocalInstance.Movement}";
    }

    public void SetActionsText()
    {
        actionsText.text = $"Actions: {Player.LocalInstance.ActionPoints}";
    }

    public void SetPointsText()
    {
        pointsText.text = $"Points: {Player.LocalInstance.Points.Value}";
    }

    public void SetPlayerNameText()
    {
        playerNameText.text = "Player Name: ";
        playerNameText.text += $"<color=#{Player.LocalInstance.HexPlayerColor}>{Player.LocalInstance.PlayerName}</color>";
    }

    public void SetActivePlayerText(Player player)
    {
        currentActivePlayerText.text = "Active player: ";
        currentActivePlayerText.text += $"<color=#{player.HexPlayerColor}>{player.PlayerName}</color>";
    }

    public void SetIsDeadText()
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

    public void SetSipCounter()
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
