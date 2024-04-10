using System.Collections.Generic;
using Unity.Netcode;

public class RollResults : NetworkBehaviour, IRollResults
{
    private List<IResult> rollResults = new List<IResult>();
    private List<ICardResults> cardBattleResults = new List<ICardResults>();

    private void Start()
    {
        IResult[] results = GetComponentsInChildren<IResult>();

        foreach (IResult result in results)
        {
            rollResults.Add(result);
        }

        ICardResults[] cardResults = GetComponentsInChildren<ICardResults>();

        foreach (ICardResults cardResult in cardResults)
        {
            cardBattleResults.Add(cardResult);
        }
    }

    public void SetRollResults(int result)
    {
        foreach (IResult rollResult in rollResults)
        {
            rollResult.SetResult(result, RollType.rollType);
        }
    }

    public void SetRollResults(List<int> results, int result)
    {
        int[] resultsArray = results.ToArray();

        foreach (ICardResults rollResult in cardBattleResults)
        {
            rollResult.SetResult(resultsArray, result, RollType.rollType);
        }
    }
}