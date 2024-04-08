public class WalkingDead : Card
{
    private int actionPointsValue = 1;
    private int resurrectionSipValue = 0;
    private int actionSipValue = 1;
    private int lastResurrectionSipValue = 0;

    public override void Equip(Player player)
    {
        lastResurrectionSipValue = player.defaultResurrectionSipValue;

        player.AddOrSubtractActionSipValue(actionSipValue);
        player.SetResurrectionSipValue(resurrectionSipValue);
        player.AddOrSubtractActionPoints(actionPointsValue);
    }

    public override void Unequip(Player player)
    {
        player.AddOrSubtractActionSipValue(-actionSipValue);
        player.SetResurrectionSipValue(lastResurrectionSipValue);
        player.AddOrSubtractActionPoints(-actionPointsValue);

        lastResurrectionSipValue = 0;
    }
}
