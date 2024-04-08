
public class DetectHidden : Card
{
    protected override int CardRollModifier => base.CardRollModifier = 1;

    public override int GetCardRollModifier()
    {
        return CardRollModifier;
    }
}
