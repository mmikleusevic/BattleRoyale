using UnityEngine;

public class Disorientation : Card
{
    public override void Equip(Player player)
    {
        if (player == Player.LocalInstance)
        {
            Vector2[][] movementVectors = GridManager.Instance.HalfMovementVectors();
            GridManager.Instance.SetMovementVectors(movementVectors);
        }
    }

    public override void Unequip(Player player)
    {
        if (player == Player.LocalInstance)
        {
            Vector2[][] movementVectors = GridManager.Instance.FullMovementVectors();
            GridManager.Instance.SetMovementVectors(movementVectors);
        }
    }
}
