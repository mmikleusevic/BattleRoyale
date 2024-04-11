public class Disarm : Card
{
    public override void OnNetworkSpawn()
    {
        Ability = GetComponent<DisarmAbility>();

        base.OnNetworkSpawn();
    }
}
