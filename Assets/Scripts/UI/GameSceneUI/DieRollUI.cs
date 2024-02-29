using System;
using UnityEngine;

public class DieRollUI : MonoBehaviour
{
    [SerializeField] private RollUI rollUI;

    private void Start()
    {
        AlcoholTypeUI.OnAlcoholButtonPress += AlcoholTypeUI_OnAlcoholButtonPress;
        InitiativeResults.OnReRoll += InitiativeResults_OnReRoll;
        InitiativeResults.OnInitiativeRollOver += InitiativeResults_OnInitiativeRollOver;
        PlayerBattleResults.OnPlayerBattleRollOver += PlayerBattleResults_OnPlayerBattleRollOver;
        PlayerBattleResults.OnPlayerBattleRoll += PlayerBattleResults_OnPlayerBattleRoll;
    }

    private void OnDestroy()
    {
        AlcoholTypeUI.OnAlcoholButtonPress -= AlcoholTypeUI_OnAlcoholButtonPress;
        InitiativeResults.OnReRoll -= InitiativeResults_OnReRoll;
        InitiativeResults.OnInitiativeRollOver -= InitiativeResults_OnInitiativeRollOver;
        PlayerBattleResults.OnPlayerBattleRollOver -= PlayerBattleResults_OnPlayerBattleRollOver;
        PlayerBattleResults.OnPlayerBattleRoll -= PlayerBattleResults_OnPlayerBattleRoll;
    }

    private void AlcoholTypeUI_OnAlcoholButtonPress()
    {
        rollUI.ShowWithAnimation();
    }

    private void InitiativeResults_OnReRoll(object sender, EventArgs e)
    {
        rollUI.ShowWithAnimation();
    }

    private void InitiativeResults_OnInitiativeRollOver(object sender, InitiativeResults.OnInitiativeRollOverEventArgs e)
    {
        rollUI.HideWithAnimation();
    }

    private void PlayerBattleResults_OnPlayerBattleRollOver(object sender, PlayerBattleResults.OnBattleRollOverEventArgs e)
    {
        rollUI.HideWithAnimation();
    }

    private void PlayerBattleResults_OnPlayerBattleRoll(object sender, EventArgs e)
    {
        rollUI.ShowWithAnimation();
    }
}
