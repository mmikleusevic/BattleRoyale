using UnityEngine;

public class TwoDiceRollUI : MonoBehaviour
{
    [SerializeField] private RollUI rollUI;

    private void Start()
    {
        PlayerBattleResults.OnPlayerBattleRollDisadvantage += PlayerBattleResults_OnPlayerBattleRollDisadvantage;
    }

    private void OnDestroy()
    {
        PlayerBattleResults.OnPlayerBattleRollDisadvantage -= PlayerBattleResults_OnPlayerBattleRollDisadvantage;
    }

    private void PlayerBattleResults_OnPlayerBattleRollDisadvantage(object sender, System.EventArgs e)
    {
        rollUI.ShowWithAnimation();
    }
}
