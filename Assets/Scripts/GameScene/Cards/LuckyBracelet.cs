public class LuckyBracelet : Card
{
    public override void OnNetworkSpawn()
    {
        Ability = GetComponent<LuckyBraceletAbility>();

        base.OnNetworkSpawn();
    }
}