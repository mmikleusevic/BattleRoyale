using System.Collections.Generic;
using System.Linq;

public class CardFilter
{
    public static CardType cardType { get; set; } = CardType.All;
    public static PagedList<Card> GetFilteredCards(IEnumerable<Card> cardList, int page, int pageSize)
    {
        if (cardType == CardType.All)
        {
            return PagedList<Card>.Create(cardList, page, pageSize);
        }

        cardList = cardList.Where(list => list.cardType == cardType);

        return PagedList<Card>.Create(cardList, page, pageSize);
    }
}
