using System.Collections;
using UnityEngine;

public class DieRollUI : RollUI
{
    public override void Awake()
    {
        base.Awake();

        GameManager.Instance.OnGameStateChanged += GameManager_OnGameStateChanged;
    }

    private void Start()
    {
        RollResults.OnReRoll += RollResults_OnReRoll;
        RollResults.OnInitiativeRollOver += RollResults_OnInitiativeRollOver;
    }
    public void OnDestroy()
    {
        GameManager.Instance.OnGameStateChanged -= GameManager_OnGameStateChanged;
        RollResults.OnReRoll -= RollResults_OnReRoll;
        RollResults.OnInitiativeRollOver -= RollResults_OnInitiativeRollOver;
    }

    private void GameManager_OnGameStateChanged(object sender, System.EventArgs e)
    {
        Show();
    }

    private void RollResults_OnReRoll(object sender, System.EventArgs e)
    {
        Show();
    }

    private void RollResults_OnInitiativeRollOver(object sender, System.EventArgs e)
    {
        StartCoroutine(DelayDisablingDice());
    }

    private IEnumerator DelayDisablingDice()
    {
        yield return new WaitForSeconds(2f);
        Hide();
    }
}
