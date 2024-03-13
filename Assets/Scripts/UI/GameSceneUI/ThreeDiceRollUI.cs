using UnityEngine;

public class ThreeDiceRollUI : MonoBehaviour
{
    [SerializeField] private RollUI rollUI;

    private void Awake()
    {
        CardBattleResults.OnCardRoll += CardBattleResults_OnCardRoll;
        CardBattleResults.OnCardWon += CardBattleResults_OnCardBattle;
        CardBattleResults.OnCardLost += CardBattleResults_OnCardBattle;
    }

    private void OnDestroy()
    {
        CardBattleResults.OnCardRoll -= CardBattleResults_OnCardRoll;
        CardBattleResults.OnCardWon -= CardBattleResults_OnCardBattle;
        CardBattleResults.OnCardLost -= CardBattleResults_OnCardBattle;
    }

    private void CardBattleResults_OnCardRoll()
    {
        rollUI.ShowWithAnimation();
    }

    private void CardBattleResults_OnCardBattle(CardBattleResults.OnCardBattleEventArgs obj)
    {
        rollUI.HideWithAnimation();
    }
}
