public class BrewmastersCurse : Card
{
    private int value = 1;

    public override void Equip(Player player)
    {
        player.AddOrSubtractResurrectionSipValue(value);
    }

    public override void Unequip(Player player)
    {
        player.AddOrSubtractResurrectionSipValue(-value);
    }
}
