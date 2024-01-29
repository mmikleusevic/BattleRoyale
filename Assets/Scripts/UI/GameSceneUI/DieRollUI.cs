using System;
using System.Collections;
using Unity.Netcode;
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

    private void RollResults_OnInitiativeRollOver(object sender, string message)
    {
        StartCoroutine(DelayDisablingDice(3f));
    }

    private IEnumerator DelayDisablingDice(float timeToDelay)
    {
        yield return new WaitForSeconds(timeToDelay);
        Hide();
    }
}
