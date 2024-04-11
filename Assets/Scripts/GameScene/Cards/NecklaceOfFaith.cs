public class NecklaceOfFaith : Card
{
    public override void OnNetworkSpawn()
    {
        Ability = GetComponent<NecklaceOfFaithAbility>();

        base.OnNetworkSpawn();
    }
}