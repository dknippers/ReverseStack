using HarmonyLib;
using ReverseStack.Extensions;

namespace ReverseStack.Patches;

/// <summary>
/// When a stack X cannot be stacked on top of a stack Y this patch will attempt the reverse: stack Y on top of X.
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
/// When dragging stack X on top of stack Y and failing to stop on top this patch will put stack Y on top of stack X instead
/// which is what the user would want in this case.
/// </summary>
[HarmonyPatch]
public static class AttemptReverseStack
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(WorldManager), nameof(WorldManager.CheckIfCanAddOnStack))]
    public static void WorldManager_CheckIfCanAddOnStack(GameCard topCard, ref bool __result)
    {
        if (__result || topCard?.CardData is null)
        {
            return;
        }

        var targetRoot = topCard
            .GetOverlappingCards()
            .Select(c => c.GetRootCard())
            .FirstOrDefault(topCard.CanReverseStackOn);

        if (targetRoot is null)
        {
            return;
        }

        // We do not want to move the target stack that we are Reverse Stacking onto.
        // By default when adding a card onto a stack on the board the stack that was being dragged will snap onto
        // the stack that is on the board.
        // Technically since we perform a Reverse Stack operation the card we are dragging is considered
        // the stack on the board and the stack that was actually on the board will be moved to snap onto the stack being dragged.
        // To fix this issue we first update the position of the stack being dragged to the position of the stack we drag onto.
        topCard.SetPosition(targetRoot.transform.position);
        targetRoot.SetParent(topCard.GetLeafCard());

        __result = true;
    }
}