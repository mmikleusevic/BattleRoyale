using System;

public class DieRollUI : RollUI
{
    public override void Awake()
    {
        base.Awake();

        Initiative.OnInitiativeStart += Initiative_OnInitiativeStart;
    }

    private void Start()
    {
        RollResults.OnReRoll += RollResults_OnReRoll;
        RollResults.OnInitiativeRollOver += RollResults_OnInitiativeRollOver;
    }

    public void OnDestroy()
    {
        Initiative.OnInitiativeStart -= Initiative_OnInitiativeStart;
        RollResults.OnReRoll -= RollResults_OnReRoll;
        RollResults.OnInitiativeRollOver -= RollResults_OnInitiativeRollOver;
    }

    private void Initiative_OnInitiativeStart(object sender, string e)
    {
        Show();
    }

    private void RollResults_OnReRoll(object sender, EventArgs e)
    {
        Show();
    }

    private void RollResults_OnInitiativeRollOver(object sender, RollResults.OnInitiativeRollOverEventArgs e)
    {
        Hide();
    }
}
