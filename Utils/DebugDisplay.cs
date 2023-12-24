using System.Text;

namespace SmartStack.Utils;

public static class DebugDisplay
{
    public static string Stack(GameCard stack)
    {
        const string CARD_PREFIX = "\n  ";

        if (stack?.CardData is null) return "<NULL>";

        var sb = new StringBuilder("\n");

        foreach(var card in stack.GetAllCardsInStack())
        {
            sb.Append(CARD_PREFIX).Append(Card(card));
        }

        sb.Append("\n\n");

        return sb.ToString();
    }

    public static string Card(GameCard card)
    {
        if (card is null) return "<NULL>";

        var name = card.CardData.Name;
        var id = card.GetInstanceID();

        return new StringBuilder("[ ").Append(name).Append(" | ").Append(id).Append(" ]").ToString();
    }
}
