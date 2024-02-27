using UnityEngine;

public class TwoDiceRollUI : MonoBehaviour
{
    [SerializeField] private RollUI rollUI;

    private void Start()
    {
        RollResults.OnPlayerBattleRollDisadvantage += RollResults_OnPlayerBattleRollDisadvantage;
    }

    private void OnDestroy()
    {
        RollResults.OnPlayerBattleRollDisadvantage -= RollResults_OnPlayerBattleRollDisadvantage;
    }

    private void RollResults_OnPlayerBattleRollDisadvantage(object sender, System.EventArgs e)
    {
        rollUI.ShowWithAnimation();
    }
}
