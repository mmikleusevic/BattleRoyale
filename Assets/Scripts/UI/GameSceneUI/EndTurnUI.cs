using UnityEngine;
using UnityEngine.UI;

public class EndTurnUI : MonoBehaviour
{
    [SerializeField] private Button endTurnButton;
    [SerializeField] private ParticleSystem particleGlow;

    private bool actionsAndMovementUsed = false;

    private void Awake()
    {
        endTurnButton.onClick.AddListener(async () =>
        {
            Hide();
            MessageUI.Instance.SendMessageToEveryoneExceptMe(SendToMessageUI());
            ParticleSystemManager.Instance.Stop(particleGlow);
            await StateManager.Instance.EndState();
        });

        PlayerTurn.OnPlayerTurn += PlayerTurn_OnPlayerTurn;
        PlayerBattleResults.OnAfterBattleResolved += PlayerBattleResults_OnAfterBattleResolved;
        Player.OnMovementOrActionPoints += Player_OnMovementOrActionPoints;
        PlayerBattleResults.OnPlayerBattleShowUI += PlayerBattleResults_OnPlayerBattleShowUI;

        ParticleSystemManager.Instance.Stop(particleGlow);

        Hide();
    }

    public void OnDestroy()
    {
        PlayerTurn.OnPlayerTurn -= PlayerTurn_OnPlayerTurn;
        PlayerBattleResults.OnAfterBattleResolved -= PlayerBattleResults_OnAfterBattleResolved;
        Player.OnMovementOrActionPoints -= Player_OnMovementOrActionPoints;
        PlayerBattleResults.OnPlayerBattleShowUI -= PlayerBattleResults_OnPlayerBattleShowUI;

        endTurnButton.onClick.RemoveAllListeners();
    }

    private void PlayerTurn_OnPlayerTurn()
    {
        endTurnButton.interactable = true;

        Show();
    }

    private void PlayerBattleResults_OnAfterBattleResolved()
    {
        endTurnButton.interactable = true;
        ToggleParticleEffect();
    }

    private void Player_OnMovementOrActionPoints()
    {
        if (Player.LocalInstance.Movement == 0 && Player.LocalInstance.ActionPoints == 0)
        {
            actionsAndMovementUsed = true;
        }
        else
        {
            actionsAndMovementUsed = false;
        }

        ToggleParticleEffect();
    }

    private void PlayerBattleResults_OnPlayerBattleShowUI(string obj)
    {
        ParticleSystemManager.Instance.Stop(particleGlow);
        endTurnButton.interactable = false;
    }

    private void ToggleParticleEffect()
    {
        if (actionsAndMovementUsed)
        {
            ParticleSystemManager.Instance.Play(particleGlow);
        }
        else
        {
            ParticleSystemManager.Instance.Stop(particleGlow);
        }
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
        actionsAndMovementUsed = false;
        gameObject.SetActive(false);
    }
}
