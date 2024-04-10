public class LuckyBracelet : Card, IPlayerReroll
{
    public void Use()
    {
        AbilityUsed = true;
    }
}
