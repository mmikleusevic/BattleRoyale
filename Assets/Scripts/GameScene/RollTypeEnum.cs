public static class RollType
{
    public static RollTypeEnum rollTypeSelf = RollTypeEnum.Initiative;
    public static RollTypeEnum rollTypeEnemy;

    public static void SetRollType()
    {
        rollTypeSelf = RollTypeEnum.CardAttack;
    }

    public static void SetRollType(Player self, Player enemy)
    {
        if (self.EquippedCards.Count > 0 && enemy.EquippedCards.Count > 0 || self.EquippedCards.Count == 0 && enemy.EquippedCards.Count == 0)
        {
            rollTypeSelf = RollTypeEnum.PlayerAttack;
            rollTypeEnemy = RollTypeEnum.PlayerAttack;
        }
        else if (self.EquippedCards.Count == 0 && enemy.EquippedCards.Count > 0)
        {
            rollTypeSelf = RollTypeEnum.Disadvantage;
            rollTypeEnemy = RollTypeEnum.PlayerAttack;
        }
        else
        {
            rollTypeSelf = RollTypeEnum.PlayerAttack;
            rollTypeEnemy = RollTypeEnum.Disadvantage;
        }
    }
}

public enum RollTypeEnum
{
    Initiative,
    Disadvantage,
    CardAttack,
    PlayerAttack
}