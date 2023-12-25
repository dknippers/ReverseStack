namespace SmartStack.Extensions;
internal static class GameCardExtensions
{
    /// <summary>
    /// Indicates if this stack can have the other stack on top, i.e. if the root of <paramref name="other"/> can be placed on the leaf of <paramref name="card"/>.
    /// </summary>
    /// <param name="card">This card / stack</param>
    /// <param name="other">The other card / stack</param>
    /// <returns></returns>
    internal static bool CanHaveOnTop(this GameCard card, GameCard other)
    {
        var leaf = card.GetLeafCard();
        var root = other.GetRootCard();

        return leaf.CardData.CanHaveCardOnTop(root.CardData);
    }

    /// <summary>
    /// Indicates if <paramref name="card"/> and <paramref name="other"/> belong to the same stack of cards.
    /// </summary>
    /// <param name="card">This card / stack</param>
    /// <param name="other">Other card / stack</param>
    /// <returns></returns>
    internal static bool IsSameStack(this GameCard card, GameCard other)
    {
        if (ReferenceEquals(card, other))
        {
            return true;
        }

        var r1 = card.GetRootCard();
        var r2 = other.GetRootCard();

        if (ReferenceEquals(r1, r2))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Indicates if <paramref name="card"/> allows a Reverse Stack operation with <paramref name="other"/>,
    /// i.e. whether <paramref name="other"/> can be stacked on top of <paramref name="card"/>.
    /// </summary>
    /// <param name="card">Card</param>
    /// <param name="other">Other card</param>
    /// <returns></returns>
    internal static bool AllowsReverseStackOn(this GameCard card, GameCard other)
    {
        if(card.CardData is Equipable || other.CardData is Equipable)
        {
            // We can never use Reverse Stack when Equipables are involved.
            return false;
        }

        return !card.IsSameStack(other) && card.CanHaveOnTop(other);
    }

    internal static bool IsRoot(this GameCard card) => card.Parent is null;
    internal static bool IsLeaf(this GameCard card) => card.Child is null;
}
