public class Shield : Card
{
    private int value = 1;

    public override int GetPlayerGameModifier()
    {
        if (!AbilityUsed)
        {
            return value;
        }

        return 0;
    }
}
