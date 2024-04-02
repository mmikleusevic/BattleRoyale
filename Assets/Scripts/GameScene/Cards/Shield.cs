using Unity.Services.Lobbies.Models;

public class Shield : Card
{
    private int value = 1;

    public override void Equip(Player player)
    {
        player.SetRollsNeededToLose(value);
    }

    public override void Unequip(Player player)
    {
        player.SetRollsNeededToLose(-value);
    }
}
