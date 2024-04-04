public class Shield : Card
{
    private int value = 1;

    public override void Equip(Player player)
    {
        player.AddOrSubtractRollsNeededToLose(value);
    }

    public override void Unequip(Player player)
    {
        player.AddOrSubtractRollsNeededToLose(-value);
    }
}
