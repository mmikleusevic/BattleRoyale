public class Sword : Card
{
    protected override int PlayerRollModifier => base.PlayerRollModifier = 1;

    public override int GetPlayerRollModifier(int result)
    {
        if (result > 1 && result < 6)
        {
            return result + CardRollModifier;
        }

        return 0;
    }
}
