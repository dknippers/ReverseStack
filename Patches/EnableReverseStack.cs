using HarmonyLib;
using SmartStack.Extensions;
using SmartStack.Utils;

namespace SmartStack.Patches;

/// <summary>
/// When an attempt to stack a stack X on top of stack Y has failed
/// this patch will check if we can instead stack Y on top of X.
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
/// This patch will do exactly that; it will attempt a reverse stack of Y on top of X when the regular stack (X on top of Y) has failed.
/// </summary>
[HarmonyPatch]
public static class EnableReverseStack
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(WorldManager), nameof(WorldManager.CheckIfCanAddOnStack))]
    public static void WorldManager_CheckIfCanAddOnStack(GameCard topCard, ref bool __result)
    {
        if (__result || topCard?.CardData is null)
        {
            return;
        }

        var inputLeaf = topCard.GetLeafCard();
        var targetRoot = topCard.GetOverlappingCards()
            .Where(card => card.IsRoot())
            .FirstOrDefault(topCard.AllowsReverseStackOn);

        if (targetRoot is null)
        {
            return;
        }

#if DEBUG
        SmartStack.ModLogger.Log($"Stack {DebugDisplay.Stack(targetRoot)} on top of {DebugDisplay.Stack(topCard)}");
#endif

        // We do not want to move the target stack that we are Reverse Stacking onto.
        // By default when adding a card onto a stack on the board the stack that was being dragged will snap onto
        // the stack that is on the board.
        // Technically since we perform a Reverse Stack operation the card we are dragging is considered
        // the stack on the board and the stack that was actually on the board will be moved to snap onto the stack being dragged.
        // To fix this issue we first update the position of the stack being dragged to the position of the stack we drag onto.
        topCard.transform.position = (topCard.TargetPosition = targetRoot.transform.position);

        targetRoot.SetParent(inputLeaf);

#if DEBUG
        SmartStack.ModLogger.Log($"New stack: {DebugDisplay.Stack(targetRoot)}");
#endif

        __result = true;
    }
}