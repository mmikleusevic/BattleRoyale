using System.Collections;
using System.Collections.Generic;

public interface IHandleCardResult
{
    IEnumerator HandleResults(List<int> resultList, List<int> diceToReroll);
}