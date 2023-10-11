using System.Collections.Generic;
using System.Linq;

public class CardFilter
{
    public CardType CardType { get; set; } = CardType.All;
    public int Page { get; set; } = 0;
    public int MaxPage { get; set; } = 0;
    public int PageSize = 8;


    public List<CardSO> GetFilteredCards(List<CardSO> cardListSO)
    {
        List<CardSO> newCardSOList;

        if (CardType == CardType.All)
        {
            MaxPage = cardListSO.Count / (PageSize + 1);

            newCardSOList = TakeAndOrderCards(cardListSO);
        }
        else
        {
            IEnumerable<CardSO> filteredCards = cardListSO.Where(list => list.CardType == CardType);

            MaxPage = filteredCards.Count() / (PageSize + 1);

            newCardSOList = TakeAndOrderCards(filteredCards);
        }

        return newCardSOList;
    }

    public List<CardSO> TakeAndOrderCards(IEnumerable<CardSO> cardListSO)
    {
        return cardListSO
            .Skip(Page * 8)
            .Take(PageSize)
            .OrderBy(list => list.Cost)
            .ToList();
    }
}
