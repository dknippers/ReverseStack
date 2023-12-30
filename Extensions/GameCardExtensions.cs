using System.Text;
using UnityEngine;

namespace ReverseStack.Extensions;
public static class GameCardExtensions
{
    /// <summary>
    /// Indicates if this stack can have the other stack on top, i.e. if the root of <paramref name="other"/> can be placed on the leaf of <paramref name="card"/>.
    /// This is similar to the built-in <see cref="CardData.CanHaveCardOnTop(CardData, bool)"/> except we also consider status cards in the stack of <paramref name="card"/>,
    /// which the game implements in a separate method <see cref="CardData.CanHaveCardsWhileHasStatus()"/>.
    /// </summary>
    /// <param name="card">This card / stack</param>
    /// <param name="other">The other card / stack</param>
    /// <returns></returns>
    public static bool CanHaveOnTop(this GameCard card, GameCard other)
    {
        var leaf = card.GetLeafCard();
        var root = other.GetRootCard();

        if (!leaf.CardData.CanHaveCardOnTop(root.CardData))
        {
            return false;
        }

        // The CanHaveCardOnTop method does not check for status cards for some reason
        // so we perform that check here to ensure `card` allows `other` to be placed on top of it.
        if (card.GetCardWithStatusInStack() is GameCard statusCard &&
           !statusCard.CardData.CanHaveCardsWhileHasStatus())
        {
            // Certain cards do not allow any cards to be placed on them when any card in the stack
            // has a status effect running right now.
            return false;
        }

        return true;
    }

    /// <summary>
    /// Indicates if <paramref name="card"/> and <paramref name="other"/> belong to the same stack of cards.
    /// </summary>
    /// <param name="card">This card / stack</param>
    /// <param name="other">Other card / stack</param>
    /// <returns></returns>
    public static bool IsSameStack(this GameCard card, GameCard other)
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
    public static bool CanReverseStackOn(this GameCard card, GameCard other)
    {
        return
            !other.IsEquipped &&
            other.Combatable?.InConflict != true &&
            !card.IsSameStack(other) &&
            card.CanHaveOnTop(other);
    }

    /// <summary>
    /// Indicates if <paramref name="card"/> allows an automatic Reverse Stack operation with <paramref name="other"/>,
    /// i.e. whether <paramref name="other"/> can be stacked on top of <paramref name="card"/> without the user performing a drag operation.
    /// This occurs for example when a Card is produced by a <see cref="Harvestable"/> or created in some other way.
    /// </summary>
    /// <param name="card">Card</param>
    /// <param name="other">Other card</param>
    /// <returns></returns>
    public static bool CanAutoReverseStackOn(this GameCard card, GameCard other)
    {
        var leaf = card.GetLeafCard();
        var root = other.GetRootCard();

        return
            leaf.IsSamePrefab(root) &&
            !other.IsWorkingOnExactBlueprint() &&
            leaf.CanReverseStackOn(root);
    }

    /// <summary>
    /// Indicates if <paramref name="card"/> is part of a stack that is currently working on a Blueprint
    /// which needs an exact match of cards to produce its result.
    /// </summary>
    /// <param name="card">Card to check</param>
    /// <returns><c>true</c> if currently working on an exact match blueprint, otherwise <c>false</c></returns>
    public static bool IsWorkingOnExactBlueprint(this GameCard? card)
    {
        if (card?.GetRootCard()?.TimerBlueprintId is not string blueprintId ||
            string.IsNullOrEmpty(blueprintId))
        {
            return false;
        }

        return WorldManager.instance.GetBlueprintWithId(blueprintId)?.NeedsExactMatch == true;
    }

    /// <summary>
    /// Reverse stacks <paramref name="card"/> onto <paramref name="target"/>.
    /// Note this method just performs the Reverse Stack without checking whether it is possible.
    /// Caller should ensure this using <see cref="CanReverseStackOn(GameCard, GameCard)"/>.
    /// </summary>
    /// <param name="card">Card to Reverse Stack</param>
    /// <param name="target">Card to Reverse Stack onto</param>
    public static void ReverseStackOn(this GameCard card, GameCard target)
    {
        // We do not want to move the target stack that we are Reverse Stacking onto.
        // By default when adding a card onto a stack on the board the stack that was being dragged will snap onto
        // the stack that is on the board.
        // Technically since we perform a Reverse Stack operation the card we are dragging is considered
        // the stack on the board and the stack that was actually on the board will be moved to snap onto the stack being dragged.
        // To fix this issue we first update the position of the stack being dragged to the position of the stack we drag onto.
        var cardLeaf = card.GetLeafCard();
        var cardRoot = card.GetRootCard();
        var targetRoot = target.GetRootCard();

        Debug.Log($"RS {cardLeaf.GetDebugName()} On {targetRoot.GetDebugName()}");

        cardRoot.SetPosition(targetRoot.transform.position);
        targetRoot.SetParent(cardLeaf);

        AudioManager.me.PlaySound2D(AudioManager.me.DropOnStack, UnityEngine.Random.Range(0.8f, 1.2f), 0.3f);
    }

    /// <summary>
    /// Instantly sets the position of <paramref name="card"/> to the given <paramref name="position"/>.
    /// </summary>
    /// <param name="card">Card</param>
    /// <param name="position">Position</param>
    public static void SetPosition(this GameCard card, Vector3 position)
    {
        // Both the underlying transform position AND the custom TargetPosition
        // properties need to be set to instantly set a card's position correctly.
        card.transform.position = card.TargetPosition = position;
    }

    /// <summary>
    /// Indicates if <paramref name="card"/> and <paramref name="other"/> are instances of the same prefab.
    /// </summary>
    /// <param name="card">Card</param>
    /// <param name="other">Other card</param>
    /// <returns></returns>
    public static bool IsSamePrefab(this GameCard card, GameCard other)
    {
        if (ReferenceEquals(card, other)) return true;

        return card.CardData.Id == other.CardData.Id;
    }

    public static bool IsRoot(this GameCard card) => card.Parent is null;
    public static bool IsLeaf(this GameCard card) => card.Child is null;

    public static string GetName(this GameCard? card)
    {
        return card?.CardData?.Name ?? "NULL";
    }

    public static string GetDebugName(this GameCard? card)
    {
        if (card is null) return "NULL";

        return new StringBuilder()
            .Append('[')
            .Append(Math.Abs(card.GetInstanceID()))
            .Append(']')
            .Append(' ')
            .Append(card.GetName())
            .ToString();
    }
}
