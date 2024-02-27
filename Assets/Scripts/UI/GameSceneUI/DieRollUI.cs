using System;
using UnityEngine;

public class DieRollUI : MonoBehaviour
{
    [SerializeField] private RollUI rollUI;

    private void Start()
    {
        AlcoholTypeUI.OnAlcoholButtonPress += AlcoholTypeUI_OnAlcoholButtonPress;
        RollResults.OnReRoll += RollResults_OnReRoll;
        RollResults.OnInitiativeRollOver += RollResults_OnInitiativeRollOver;
        RollResults.OnBattleRollOver += RollResults_OnBattleRollOver;
        RollResults.OnPlayerBattleRoll += RollResults_OnPlayerBattleRoll;
    }

    private void OnDestroy()
    {
        AlcoholTypeUI.OnAlcoholButtonPress -= AlcoholTypeUI_OnAlcoholButtonPress;
        RollResults.OnReRoll -= RollResults_OnReRoll;
        RollResults.OnInitiativeRollOver -= RollResults_OnInitiativeRollOver;
        RollResults.OnBattleRollOver -= RollResults_OnBattleRollOver;
        RollResults.OnPlayerBattleRoll -= RollResults_OnPlayerBattleRoll;
    }

    private void AlcoholTypeUI_OnAlcoholButtonPress()
    {
        rollUI.ShowWithAnimation();
    }

    private void RollResults_OnReRoll(object sender, EventArgs e)
    {
        rollUI.ShowWithAnimation();
    }

    private void RollResults_OnInitiativeRollOver(object sender, RollResults.OnInitiativeRollOverEventArgs e)
    {
        rollUI.HideWithAnimation();
    }

    private void RollResults_OnBattleRollOver(object sender, RollResults.OnBattleRollOverEventArgs e)
    {
        rollUI.HideWithAnimation();
    }

    private void RollResults_OnPlayerBattleRoll(object sender, EventArgs e)
    {
        rollUI.ShowWithAnimation();
    }
}
