using System;
using TMPro;
using UnityEngine;

public class DieRollUI : MonoBehaviour
{
    [SerializeField] private RollUI rollUI;

    private void Start()
    {
        AlcoholTypeUI.OnAlcoholButtonPress += AlcoholTypeUI_OnAlcoholButtonPress;
        InitiativeResults.OnReRoll += InitiativeResults_OnReRoll;
        InitiativeResults.OnInitiativeRollOver += InitiativeResults_OnInitiativeRollOver;
        PlayerBattleResults.OnPlayerBattleRollDie += PlayerBattleResults_OnPlayerBattleRoll;
        PlayerBattleResults.OnPlayerBattleRollDieOver += PlayerBattleResults_OnPlayerBattleRollDieOver;
    }

    private void OnDestroy()
    {
        AlcoholTypeUI.OnAlcoholButtonPress -= AlcoholTypeUI_OnAlcoholButtonPress;
        InitiativeResults.OnReRoll -= InitiativeResults_OnReRoll;
        InitiativeResults.OnInitiativeRollOver -= InitiativeResults_OnInitiativeRollOver;
        PlayerBattleResults.OnPlayerBattleRollDie -= PlayerBattleResults_OnPlayerBattleRoll;
        PlayerBattleResults.OnPlayerBattleRollDieOver -= PlayerBattleResults_OnPlayerBattleRollDieOver;
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

    private void PlayerBattleResults_OnPlayerBattleRollDieOver()
    {
        rollUI.HideWithAnimation();
    }

    private void PlayerBattleResults_OnPlayerBattleRoll()
    {
        rollUI.ShowWithAnimation();
    }
}
