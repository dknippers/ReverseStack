using HarmonyLib;
using ReverseStack.Extensions;
using UnityEngine;

namespace ReverseStack.Patches;

/// <summary>
/// When dragging a stack X that cannot be stacked on top of a stack Y this patch will attempt the reverse: stack Y on top of X.
///
/// This is especially useful when stack Y contains a Villager harvesting cards such as Apple Trees and does not allow any
/// cards being stacked on top of it, but it can itself be stacked on top of another stack.
/// 
/// For example, consider stack X being:
///
/// <code>
/// [ Apple Tree ]
///
/// and stack Y being:
/// 
/// [ Apple Tree ] &lt;- bottom of stack
/// [ Apple Tree ]
/// [ Villager   ] &lt;- top of stack
/// </code>
/// 
/// Stack X is not allowed on top of stack Y because the game does not allow anything to be added on top of
/// a stack that has any status cards (cards that have a timer running). However, it is perfectly valid to put stack Y on top of stack X
/// even if Y contains status cards.
///
/// When dragging stack X on top of stack Y and failing to stack on top this patch will put stack Y on top of stack X instead
/// which is what the user would want in this case.
/// </summary>
[HarmonyPatch]
public static class EnableReverseStackOnDrag
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(WorldManager), nameof(WorldManager.CheckIfCanAddOnStack))]
    public static void WorldManager_CheckIfCanAddOnStack(GameCard topCard, ref bool __result)
    {
        if (__result || topCard?.CardData is null)
        {
            return;
        }

        var targetRoot = FindOverlappingTarget(topCard);

        if (targetRoot is null)
        {
            return;
        }

        topCard.ReverseStackOn(targetRoot);

        __result = true;
    }

    private static GameCard? FindOverlappingTarget(GameCard card)
    {
        var cards = card.GetAllCardsInStack();
        var (center, size) = CombineColliders(cards);
        var overlappingCards = card.GetOverlappingCardsInBox(center, size);

        foreach (var oc in overlappingCards)
        {
            var root = oc.GetRootCard();

            if (card.CanReverseStackOn(root))
            {
                return root;
            }
        }

        return null;
    }

    private static (Vector3 center, Vector3 size) CombineColliders(IEnumerable<GameCard> cards)
    {
        var colliders = cards.Select(c => c.boxCollider).ToArray();

        Vector3 minExtent = colliders[0].bounds.min;
        Vector3 maxExtent = colliders[0].bounds.max;

        for (int i = 1; i < colliders.Length; i++)
        {
            minExtent = Vector3.Min(minExtent, colliders[i].bounds.min);
            maxExtent = Vector3.Max(maxExtent, colliders[i].bounds.max);
        }

        Vector3 center = (minExtent + maxExtent) / 2f;
        Vector3 size = maxExtent - minExtent;

        return (center, size);
    }
}