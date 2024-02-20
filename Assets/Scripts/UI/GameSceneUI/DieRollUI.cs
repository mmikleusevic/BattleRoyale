using System;
using Unity.Netcode;
using UnityEngine;

public class DieRollUI : MonoBehaviour
{
    [SerializeField] private RollUI rollUI;

    private void Awake()
    {
        Initiative.OnInitiativeStart += Initiative_OnInitiativeStart;
    }

    private void Start()
    {
        RollResults.OnReRoll += RollResults_OnReRoll;
        RollResults.OnInitiativeRollOver += RollResults_OnInitiativeRollOver;
        RollResults.OnBattleRollOver += RollResults_OnBattleRollOver;
        RollResults.OnPlayerBattleRoll += RollResults_OnPlayerBattleRoll;
    }

    private void OnDestroy()
    {
        Initiative.OnInitiativeStart -= Initiative_OnInitiativeStart;
        RollResults.OnReRoll -= RollResults_OnReRoll;
        RollResults.OnInitiativeRollOver -= RollResults_OnInitiativeRollOver;
        RollResults.OnBattleRollOver -= RollResults_OnBattleRollOver;
        RollResults.OnPlayerBattleRoll -= RollResults_OnPlayerBattleRoll;
    }

    private void Initiative_OnInitiativeStart(object sender, string e)
    {
        rollUI.Show();
    }

    private void RollResults_OnReRoll(object sender, EventArgs e)
    {
        rollUI.Show();
    }

    private void RollResults_OnInitiativeRollOver(object sender, RollResults.OnInitiativeRollOverEventArgs e)
    {
        rollUI.Hide();
    }

    private void RollResults_OnBattleRollOver(object sender, RollResults.OnBattleRollOverEventArgs e)
    {
        rollUI.Hide();
    }

    private void RollResults_OnPlayerBattleRoll(object sender, EventArgs e)
    {
        rollUI.Show();
    }
}
