using System.Collections.Generic;
using System.Linq;

public class CardFilter
{
    public static CardType CardType { get; set; } = CardType.All;
    public static PagedList<Card> GetFilteredCards(IEnumerable<Card> cardList, int page, int pageSize)
    {
        if (CardType == CardType.All)
        {
            return PagedList<Card>.CreateAsync(cardList, page, pageSize);
        }

        cardList = cardList.Where(list => list.CardType == CardType);

        return PagedList<Card>.CreateAsync(cardList, page, pageSize);
    }
}
