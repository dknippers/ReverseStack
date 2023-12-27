using HarmonyLib;
using ReverseStack.Extensions;
using UnityEngine;

namespace ReverseStack.Patches;

/// <summary>
/// Highlight targets for a Reverse Stack operation.
/// For an explanation of a Reverse Stack see <see cref="AttemptReverseStack"/>.
/// </summary>
[HarmonyPatch]
public static class HighlightReverseStackTargets
{
    private const bool DEFAULT_DASHED = true;
    private const float DEFAULT_CORNER_RADIUS = 0f;
    private const float DEFAULT_THICKNESS = 0.022f;
    private const float DEFAULT_HEIGHT = 0.5171013f;
    private const float DEFAULT_WIDTH = 0.435f;

    private const bool RS_DASHED = false;
    private const float RS_CORNER_RADIUS = 0.02f;
    private const float RS_THICKNESS = 0.032f;
    private const float RS_ALPHA = 0.8f;

    private const float RS_PADDING = -0.01f;
    private const float RS_DELTA = RS_THICKNESS - DEFAULT_THICKNESS;
    private const float RS_HEIGHT = DEFAULT_HEIGHT + RS_DELTA + RS_PADDING;
    private const float RS_WIDTH = DEFAULT_WIDTH + RS_DELTA + RS_PADDING;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameCard), "Update")]
    public static void GameCard_Update(GameCard __instance)
    {
        if (__instance is not GameCard card || card.HighlightActive) return;

        if (WorldManager.instance.DraggingCard is not GameCard draggingCard ||
            !card.IsRoot() ||
            card.CanHaveOnTop(draggingCard) ||
            !draggingCard.CanReverseStackOn(card))
        {
            if (card.HighlightRectangle.Dashed != DEFAULT_DASHED)
            {
                // Revert our custom highlighting style if it is still applied
                card.HighlightRectangle.Dashed = DEFAULT_DASHED;
                card.HighlightRectangle.CornerRadius = DEFAULT_CORNER_RADIUS;
                card.HighlightRectangle.Thickness = DEFAULT_THICKNESS;
                card.HighlightRectangle.Height = DEFAULT_HEIGHT;
                card.HighlightRectangle.Width = DEFAULT_WIDTH;
            }

            return;
        }

        // Reverse Stack highlight
        card.HighlightActive = true;
        card.HighlightRectangle.enabled = true;
        card.HighlightRectangle.Dashed = RS_DASHED;
        card.HighlightRectangle.Color = new Color(0, 0, 0, RS_ALPHA);
        card.HighlightRectangle.CornerRadius = RS_CORNER_RADIUS;
        card.HighlightRectangle.Thickness = RS_THICKNESS;
        card.HighlightRectangle.Height = RS_HEIGHT;
        card.HighlightRectangle.Width = RS_WIDTH;
    }
}
