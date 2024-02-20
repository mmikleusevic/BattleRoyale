public static class RollType
{
    public static RollTypeEnum rollType = RollTypeEnum.Initiative;
}

public enum RollTypeEnum
{
    Initiative,
    Disadvantage,
    CardAttack,
    PlayerAttack
}