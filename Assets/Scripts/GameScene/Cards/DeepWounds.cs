using System.Collections;
using System.Collections.Generic;

public class DeepWounds : Card
{
    public IEnumerator Ability(List<int> resultList, List<int> diceToReroll)
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
}
