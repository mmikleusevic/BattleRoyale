using UnityEngine;

public class ThreeDiceRollUI : MonoBehaviour
{
    [SerializeField] private RollUI rollUI;

    private void Awake()
    {
        RollResults.OnPlayerCardRoll += RollResults_OnPlayerCardRoll;
        RollResults.OnPlayerCardWon += RollResults_OnPlayerCardWon;
        RollResults.OnPlayerCardLost += RollResults_OnPlayerCardLost;
    }

    private void OnDestroy()
    {
        RollResults.OnPlayerCardRoll -= RollResults_OnPlayerCardRoll;
        RollResults.OnPlayerCardWon -= RollResults_OnPlayerCardWon;
        RollResults.OnPlayerCardLost -= RollResults_OnPlayerCardLost;
    }

    private void RollResults_OnPlayerCardRoll()
    {
        rollUI.ShowWithAnimation();
    }
    private void RollResults_OnPlayerCardLost(string message)
    {
        rollUI.HideWithAnimation();
    }

    private void RollResults_OnPlayerCardWon(RollResults.OnCardWonEventArgs obj)
    {
        rollUI.HideWithAnimation();
    }
}
