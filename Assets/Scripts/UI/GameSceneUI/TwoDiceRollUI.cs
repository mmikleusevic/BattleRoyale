using UnityEngine;

public class TwoDiceRollUI : MonoBehaviour
{
    [SerializeField] private RollUI rollUI;


    private void Start()
    {
        PlayerBattleResults.OnPlayerBattleRollDisadvantage += PlayerBattleResults_OnPlayerBattleRollDisadvantage;
        PlayerBattleResults.OnPlayerBattleRollDisadvantageRollOver += PlayerBattleResults_OnPlayerBattleRollDisadvantageRollOver;
    }

    private void OnDestroy()
    {
        PlayerBattleResults.OnPlayerBattleRollDisadvantage -= PlayerBattleResults_OnPlayerBattleRollDisadvantage;
        PlayerBattleResults.OnPlayerBattleRollDisadvantageRollOver -= PlayerBattleResults_OnPlayerBattleRollDisadvantageRollOver;
    }

    private void PlayerBattleResults_OnPlayerBattleRollDisadvantage()
    {
        rollUI.ShowWithAnimation();
    }

    private void PlayerBattleResults_OnPlayerBattleRollDisadvantageRollOver()
    {
        rollUI.HideWithAnimation();
    }
}
