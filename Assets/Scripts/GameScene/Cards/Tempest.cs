public class Tempest : Card
{
    private AbilityResults abilityResults;

    public override void OnNetworkSpawn()
    {
        RollResults rollResults = FindFirstObjectByType<RollResults>();

        abilityResults = rollResults.GetComponentInChildren<AbilityResults>();

        Ability = new TempestAbility(this, abilityResults);

        base.OnNetworkSpawn();
    }
}
