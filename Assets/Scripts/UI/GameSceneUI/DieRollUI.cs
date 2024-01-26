using System;
using System.Collections;
using UnityEngine;

public class DieRollUI : RollUI
{
    public override void Awake()
    {
        base.Awake();

        GameManager.Instance.OnGameStarted += GameManager_OnGameStarted;
    }

    private void Start()
    {
        RollResults.OnReRoll += RollResults_OnReRoll;
        RollResults.OnInitiativeRollOver += RollResults_OnInitiativeRollOver;
    }
    public void OnDestroy()
    {
        GameManager.Instance.OnGameStarted -= GameManager_OnGameStarted;
        RollResults.OnReRoll -= RollResults_OnReRoll;
        RollResults.OnInitiativeRollOver -= RollResults_OnInitiativeRollOver;
    }

    private void GameManager_OnGameStarted(object sender, EventArgs e)
    {
        Show();
    }

    private void RollResults_OnReRoll(object sender, EventArgs e)
    {
        Show();
    }

    private void RollResults_OnInitiativeRollOver(object sender, EventArgs e)
    {
        StartCoroutine(DelayDisablingDice());
    }

    private IEnumerator DelayDisablingDice()
    {
        yield return new WaitForSeconds(2f);
        Hide();
    }
}
