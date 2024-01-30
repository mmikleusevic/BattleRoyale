using System.Collections;

public abstract class State
{
    public virtual IEnumerator Start()
    {
        yield break;
    }
}
