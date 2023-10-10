using System.Collections.Generic;
using System.Linq;

public class CardFilter
{
    public CardType CardType { get; set; } = CardType.All;

    public List<CardSO> GetFilteredCards(List<CardSO> cardListSO)
    {
        if (CardType == CardType.All) return cardListSO;

        return cardListSO.Where(list => list.CardType == CardType).OrderBy(list => list.Cost).ToList();
    }
}
