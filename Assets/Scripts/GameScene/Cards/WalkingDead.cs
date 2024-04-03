public class WalkingDead : Card
{
    private int sipValue = 1;
    private int actionPointsValue = 3;
    private int addOrSubtractActionPointsValue = 1;

    public override void Equip(Player player)
    {
        player.AddOrSubtractActionSipValue(sipValue);
        player.AddOrSubtractSipValue(sipValue);
        player.SetActionPoints(actionPointsValue);
        player.SetDefaultActionPoints(addOrSubtractActionPointsValue);
    }

    public override void Unequip(Player player)
    {
        player.AddOrSubtractActionSipValue(-sipValue);
        player.AddOrSubtractSipValue(-sipValue);
        player.SubtractActionPoints(-addOrSubtractActionPointsValue);
        player.SetDefaultActionPoints(-addOrSubtractActionPointsValue);
    }
}
