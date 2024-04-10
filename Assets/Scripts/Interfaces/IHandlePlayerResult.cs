using System.Collections;
using System.Collections.Generic;

public interface IHandlePlayerResult
{
    IEnumerator HandleResults(int result, List<int> diceToReroll);
}