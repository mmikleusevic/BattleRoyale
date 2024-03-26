using System;
using UnityEngine;

public class DiceRollUI : MonoBehaviour
{
    [SerializeField] private RollUI rollUI;

    private void Awake()
    {
        CardBattleResults.OnCardRoll += CardBattleResults_OnCardRoll;
        CardBattleResults.OnCardWon += CardBattleResults_OnCardBattle;
        CardBattleResults.OnCardLost += CardBattleResults_OnCardBattle;
        PlayerBattleResults.OnPlayerBattleRollDisadvantage += PlayerBattleResults_OnPlayerBattleRollDisadvantage;
        AlcoholTypeUI.OnAlcoholButtonPress += AlcoholTypeUI_OnAlcoholButtonPress;
        InitiativeResults.OnReRoll += InitiativeResults_OnReRoll;
        InitiativeResults.OnInitiativeRollOver += InitiativeResults_OnInitiativeRollOver;
        PlayerBattleResults.OnPlayerBattleRollDie += PlayerBattleResults_OnPlayerBattleRoll;
        PlayerBattleResults.OnPlayerBattleRollDieOver += PlayerBattleResults_OnPlayerBattleRollDieOver;
    }

    private void OnDestroy()
    {
        CardBattleResults.OnCardRoll -= CardBattleResults_OnCardRoll;
        CardBattleResults.OnCardWon -= CardBattleResults_OnCardBattle;
        CardBattleResults.OnCardLost -= CardBattleResults_OnCardBattle;
        PlayerBattleResults.OnPlayerBattleRollDisadvantage -= PlayerBattleResults_OnPlayerBattleRollDisadvantage;
        AlcoholTypeUI.OnAlcoholButtonPress -= AlcoholTypeUI_OnAlcoholButtonPress;
        InitiativeResults.OnReRoll -= InitiativeResults_OnReRoll;
        InitiativeResults.OnInitiativeRollOver -= InitiativeResults_OnInitiativeRollOver;
        PlayerBattleResults.OnPlayerBattleRollDie -= PlayerBattleResults_OnPlayerBattleRoll;
        PlayerBattleResults.OnPlayerBattleRollDieOver -= PlayerBattleResults_OnPlayerBattleRollDieOver;
    }

    private void CardBattleResults_OnCardRoll()
    {
        rollUI.ShowWithAnimation(3);
    }

    private void CardBattleResults_OnCardBattle(CardBattleResults.OnCardBattleEventArgs obj)
    {
        rollUI.HideWithAnimation();
    }

    private void PlayerBattleResults_OnPlayerBattleRollDisadvantage()
    {
        rollUI.ShowWithAnimation(2);
    }

    private void AlcoholTypeUI_OnAlcoholButtonPress()
    {
        rollUI.ShowWithAnimation(1);
    }

    private void InitiativeResults_OnReRoll(object sender, EventArgs e)
    {
        rollUI.ShowWithAnimation(1);
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
        rollUI.ShowWithAnimation(1);
    }
}
