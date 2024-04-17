public class SpaceShift : Card
{
    public override void OnNetworkSpawn()
    {
        Ability = GetComponent<SpaceShiftAbility>();

        base.OnNetworkSpawn();
    }
}