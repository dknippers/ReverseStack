using HarmonyLib;
using SmartStack.Extensions;
using UnityEngine;

namespace SmartStack.Patches;

/// <summary>
/// Highlight targets for a Reverse Stack operation.
/// For an explanation of a Reverse Stack see <see cref="AttemptReverseStackAfterFailedStack"/>.
/// </summary>
[HarmonyPatch]
public static class HighlightReverseStackTargets
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameCard), "Update")]
    public static void GameCard_Update(GameCard __instance)
    {
        if (__instance is not GameCard card ||
            card.HighlightRectangle.enabled ||
            WorldManager.instance.DraggingCard is not GameCard draggingCard ||
            !card.IsRoot() ||
            card.CanHaveOnTop(draggingCard) ||
            !draggingCard.AllowsReverseStackOn(card))
        {
            return;
        }

        // Reverse Stack highlight
        card.HighlightRectangle.enabled = true;
        card.HighlightRectangle.Color = Color.black;
    }
}