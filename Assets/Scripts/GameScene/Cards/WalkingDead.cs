public class WalkingDead : Card
{
    private int sipValue = 1;
    private int actionPointsValue = 3;

    public override void Equip(Player player)
    {
        player.AddOrSubtractActionSipValue(sipValue);
        player.AddOrSubtractSipValue(sipValue);
        player.SetActionPoints(actionPointsValue);
    }

    public override void Unequip(Player player)
    {
        player.AddOrSubtractActionSipValue(-sipValue);
        player.AddOrSubtractSipValue(-sipValue);
        player.SetActionPoints(-actionPointsValue);
    }
}
