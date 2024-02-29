using UnityEngine;

public class ThreeDiceRollUI : MonoBehaviour
{
    [SerializeField] private RollUI rollUI;

    private void Awake()
    {
        CardBattleResults.OnCardRoll += CardBattleResults_OnCardRoll;
        CardBattleResults.OnCardWon += CardBattleResults_OnCardWon;
        CardBattleResults.OnCardLost += CardBattleResults_OnCardLost;
    }

    private void OnDestroy()
    {
        CardBattleResults.OnCardRoll -= CardBattleResults_OnCardRoll;
        CardBattleResults.OnCardWon -= CardBattleResults_OnCardWon;
        CardBattleResults.OnCardLost -= CardBattleResults_OnCardLost;
    }

    private void CardBattleResults_OnCardRoll()
    {
        rollUI.ShowWithAnimation();
    }

    private void CardBattleResults_OnCardWon(CardBattleResults.OnCardWonEventArgs obj)
    {
        rollUI.HideWithAnimation();
    }

    private void CardBattleResults_OnCardLost(string obj)
    {
        rollUI.HideWithAnimation();
    }
}
