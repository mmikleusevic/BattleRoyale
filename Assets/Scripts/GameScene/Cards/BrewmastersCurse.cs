public class BrewmastersCurse : Card
{
    private int value = 1;

    public override void Equip(Player player)
    {
        player.AddOrSubtractSipValue(value);
    }

    public override void Unequip(Player player)
    {
        player.AddOrSubtractSipValue(-value);
    }
}
