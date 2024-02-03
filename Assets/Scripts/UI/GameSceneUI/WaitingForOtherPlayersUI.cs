using UnityEngine;

public class WaitingForOtherPlayersUI : MonoBehaviour
{
    private void Awake()
    {
        Initiative.OnInitiativeStart += Initiative_OnInitiativeStart;
    }

    private void OnDestroy()
    {
        Initiative.OnInitiativeStart -= Initiative_OnInitiativeStart;
    }

    private void Initiative_OnInitiativeStart(object sender, string e)
    {
        Hide();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
