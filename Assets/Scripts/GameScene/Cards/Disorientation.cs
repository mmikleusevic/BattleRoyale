
public class Disorientation : Card
{
    public override void Equip(Player player)
    {
        if (player == Player.LocalInstance)
        {
            GridManager.Instance.HalfMovementVectors();
        }
    }

    public override void Unequip(Player player)
    {
        if (player == Player.LocalInstance)
        {
            GridManager.Instance.FullMovementVectors();
        }
    }
}
