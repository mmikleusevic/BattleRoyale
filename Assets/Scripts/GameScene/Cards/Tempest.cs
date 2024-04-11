public class Tempest : Card
{
    private AbilityResults abilityResults;

    public override void OnNetworkSpawn()
    {
        RollResults rollResults = FindFirstObjectByType<RollResults>();

        abilityResults = rollResults.GetComponentInChildren<AbilityResults>();

        TempestAbility tempestAbility = GetComponent<TempestAbility>();
        tempestAbility.SetAbilityResults(abilityResults);

        Ability = tempestAbility;

        base.OnNetworkSpawn();
    }
}
