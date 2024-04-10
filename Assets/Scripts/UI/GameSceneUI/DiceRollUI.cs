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
        PlayerBattleResults.OnPlayerBattleRollDieOver += Hide;
        AbilityResults.OnStartRoll += TempestAbility_OnStartRoll;
        AbilityResults.OnEndRoll += Hide;
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
        PlayerBattleResults.OnPlayerBattleRollDieOver -= Hide;
        AbilityResults.OnStartRoll -= TempestAbility_OnStartRoll;
        AbilityResults.OnEndRoll += Hide;
    }

    private void CardBattleResults_OnCardRoll()
    {
        rollUI.ShowWithAnimation(3, BattleType.Card);
    }

    private void CardBattleResults_OnCardBattle(Card card)
    {
        Hide();
    }

    private void PlayerBattleResults_OnPlayerBattleRollDisadvantage()
    {
        rollUI.ShowWithAnimation(2, BattleType.Player);
    }

    private void AlcoholTypeUI_OnAlcoholButtonPress()
    {
        rollUI.ShowWithAnimation(1, BattleType.Player);
    }

    private void InitiativeResults_OnReRoll()
    {
        rollUI.ShowWithAnimation(1, BattleType.Player);
    }

    private void TempestAbility_OnStartRoll()
    {
        rollUI.ShowWithAnimation(1, BattleType.Ability);
    }

    private void InitiativeResults_OnInitiativeRollOver(InitiativeResults.OnInitiativeRollOverEventArgs e)
    {
        Hide();
    }

    private void Hide()
    {
        rollUI.HideWithAnimation();
    }

    private void PlayerBattleResults_OnPlayerBattleRoll()
    {
        rollUI.ShowWithAnimation(1, BattleType.Player);
    }
}
