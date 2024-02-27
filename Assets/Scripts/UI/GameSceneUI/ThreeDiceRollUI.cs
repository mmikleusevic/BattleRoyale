using UnityEngine;

public class ThreeDiceRollUI : MonoBehaviour
{
    [SerializeField] private RollUI rollUI;

    private void Awake()
    {
        RollResults.OnPlayerCardRoll += RollResults_OnPlayerCardRoll;
    }

    private void OnDestroy()
    {
        RollResults.OnPlayerCardRoll -= RollResults_OnPlayerCardRoll;
    }

    private void RollResults_OnPlayerCardRoll()
    {
        rollUI.ShowWithAnimation();
    }
}
