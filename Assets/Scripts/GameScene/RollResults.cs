using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RollResults : NetworkBehaviour, IRollResults
{
    [SerializeField] private InitiativeResults initiativeResults;
    [SerializeField] private PlayerBattleResults playerBattleResults;
    [SerializeField] private CardBattleResults cardBattleResults;

    public void SetRollResults(int result, RollTypeEnum rollType)
    {
        switch (rollType)
        {
            case RollTypeEnum.Initiative:
                initiativeResults.SetInitiativeResultServerRpc(result);
                break;
            case RollTypeEnum.PlayerAttack:
            case RollTypeEnum.Disadvantage:
                playerBattleResults.SetBattleResultServerRpc(result);
                break;
        }
    }

    public void SetRollResults(List<int> result)
    {
        cardBattleResults.SetCardResult(result);
    }
}