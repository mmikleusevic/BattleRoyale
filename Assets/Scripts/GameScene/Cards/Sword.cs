public class Sword : Card
{
    protected override int PlayerRollModifier => base.PlayerRollModifier = 1;

    public override int GetPlayerRollModifier(int result)
    {
        if (!AbilityUsed)
        {
            if (result > 1 && result < 6)
            {
                return PlayerRollModifier;
            }

        }

        return 0;
    }
}
