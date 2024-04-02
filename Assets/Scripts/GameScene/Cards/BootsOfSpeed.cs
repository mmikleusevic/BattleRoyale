public class BootsOfSpeed : Card
{
    private int value = 1;

    public override void Equip(Player player)
    {
        player.SetMovement(value);
    }

    public override void Unequip(Player player)
    {
        player.SetMovement(-value);
    }
}
