public class BlackDeath : Card
{
    public override void OnNetworkSpawn()
    {
        Ability = GetComponent<BlackDeathAbility>();

        base.OnNetworkSpawn();
    }
}
