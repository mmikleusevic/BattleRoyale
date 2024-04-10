using System.Collections;
using System.Collections.Generic;

public class DeepWounds : Card, IHandlePlayerResult, IHandleCardResult
{
    public IEnumerator HandleResults(List<int> resultList, List<int> diceToReroll)
    {
        diceToReroll.Clear();

        for (int i = 0; i < resultList.Count; i++)
        {
            if (resultList[i] == 1 || resultList[i] == 6)
            {
                diceToReroll.Add(i);
            }
        }

        yield return diceToReroll;
    }

    public IEnumerator HandleResults(int result, List<int> diceToReroll)
    {
        diceToReroll.Clear();

        if (result == 1 || result == 6)
        {
            diceToReroll.Add(0);
        }

        yield return diceToReroll;
    }
}