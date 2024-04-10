using System.Collections.Generic;

public interface IRollResults
{
    void SetRollResults(int result);
    void SetRollResults(List<int> results, int result);
}