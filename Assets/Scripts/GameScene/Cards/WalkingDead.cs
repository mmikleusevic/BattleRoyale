public class WalkingDead : Card
{
    private int sipValue = 1;
    private int actionPointsValue = 1;

    public override void Equip(Player player)
    {
        player.AddOrSubtractActionSipValue(sipValue);
        player.AddOrSubtractSipValue(sipValue);
        player.AddOrSubtractActionPoints(actionPointsValue);
    }

    public override void Unequip(Player player)
    {
        player.AddOrSubtractActionSipValue(-sipValue);
        player.AddOrSubtractSipValue(-sipValue);
        player.AddOrSubtractActionPoints(-actionPointsValue);
    }
}
