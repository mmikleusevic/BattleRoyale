public static class RollType
{
    public static RollTypeEnum rollType = RollTypeEnum.Initiative;

    public static void ResetStaticData()
    {
        rollType = RollTypeEnum.Initiative;
    }
}

public enum RollTypeEnum
{
    Initiative,
    Disadvantage,
    CardAttack,
    PlayerAttack,
    Ability
}