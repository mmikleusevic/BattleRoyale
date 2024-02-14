using System;

public class DieRollUI : RollUI
{
    public override void Awake()
    {
        base.Awake();

        Initiative.OnInitiativeStart += Initiative_OnInitiativeStart;
        AttackPlayerInfoUI.OnAttackPlayer += AttackPlayerInfoUI_OnAttackPlayer;
    }

    private void Start()
    {
        RollResults.OnReRoll += RollResults_OnReRoll;
        RollResults.OnInitiativeRollOver += RollResults_OnInitiativeRollOver;
        RollResults.OnBattleRollOver += RollResults_OnBattleRollOver;
    }

    public void OnDestroy()
    {
        Initiative.OnInitiativeStart -= Initiative_OnInitiativeStart;
        RollResults.OnReRoll -= RollResults_OnReRoll;
        RollResults.OnInitiativeRollOver -= RollResults_OnInitiativeRollOver;
        RollResults.OnBattleRollOver -= RollResults_OnBattleRollOver;
        AttackPlayerInfoUI.OnAttackPlayer -= AttackPlayerInfoUI_OnAttackPlayer;
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

    private void RollResults_OnBattleRollOver(object sender, RollResults.OnBattleRollOverEventArgs e)
    {
        Hide();
    }

    private void AttackPlayerInfoUI_OnAttackPlayer(AttackPlayerInfoUI.OnAttackPlayerEventArgs obj)
    {
        Show();
    }
}
