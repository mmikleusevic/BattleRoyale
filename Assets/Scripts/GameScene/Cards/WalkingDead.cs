public class WalkingDead : Card
{
    private int actionPointsValue = 1;
    private int actionSipValue = 1;
    private int defaultResurrectionSipValue = 0;

    public override void Equip(Player player)
    {
        defaultResurrectionSipValue = player.defaultResurrectionSipValue;

        player.AddOrSubtractActionSipValue(actionSipValue);
        player.AddOrSubtractResurrectionSipValue(-defaultResurrectionSipValue);
        player.AddOrSubtractActionPoints(actionPointsValue);
    }

    public override void Unequip(Player player)
    {
        player.AddOrSubtractActionSipValue(-actionSipValue);
        player.AddOrSubtractResurrectionSipValue(defaultResurrectionSipValue);
        player.AddOrSubtractActionPoints(-actionPointsValue);

        defaultResurrectionSipValue = 0;
    }
}
