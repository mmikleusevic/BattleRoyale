using System.Collections.Generic;
using Unity.Netcode;

public class RollResults : NetworkBehaviour, IRollResults
{
    private List<IResult> rollResults = new List<IResult>();
    private List<ICardResults> cardBattleResults = new List<ICardResults>();

    private void Start()
    {
        IResult[] iResults = GetComponentsInChildren<IResult>();

        foreach (IResult iResult in iResults)
        {
            rollResults.Add(iResult);
        }

        ICardResults[] iCardResults = GetComponentsInChildren<ICardResults>();

        foreach (ICardResults iCardResult in iCardResults)
        {
            cardBattleResults.Add(iCardResult);
        }
    }

    public void SetRollResults(int result)
    {
        foreach (IResult rollResult in rollResults)
        {
            rollResult.SetResultServerRpc(result, RollType.rollType);
        }
    }

    public void SetRollResults(List<int> results, int result)
    {
        int[] resultsArray = results.ToArray();

        foreach (ICardResults rollResult in cardBattleResults)
        {
            rollResult.SetResultServerRpc(resultsArray, result, RollType.rollType);
        }
    }
}