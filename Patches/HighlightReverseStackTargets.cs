using HarmonyLib;
using ReverseStack.Extensions;
using Shapes;
using UnityEngine;

namespace ReverseStack.Patches;

/// <summary>
/// Highlight targets for a Reverse Stack operation.
/// For an explanation of a Reverse Stack see <see cref="AttemptReverseStack"/>.
/// </summary>
[HarmonyPatch]
internal static class HighlightReverseStackTargets
{
    internal static HighlightValues Original = null!;
    internal static HighlightValues Modified = null!;

    private const bool RS_DASHED = false;
    private const float RS_CORNER_RADIUS = 0.02f;
    private const float RS_THICKNESS = 0.032f;
    private const float RS_ALPHA = 0.8f;
    private const float RS_PADDING = -0.01f;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameCard), "Update")]
    internal static void GameCard_Update(GameCard __instance)
    {
        if (__instance is not GameCard card || card.HighlightActive) return;

        if (WorldManager.instance.DraggingCard is not GameCard draggingCard ||
            !card.IsRoot() ||
            card.CanHaveOnTop(draggingCard) ||
            !draggingCard.CanReverseStackOn(card))
        {
            if (!Original.IsApplied(card.HighlightRectangle))
            {
                Original.Apply(card.HighlightRectangle);
            }

            return;
        }

        // Reverse Stack highlight
        card.HighlightActive = true;
        card.HighlightRectangle.enabled = true;

        card.HighlightRectangle.Color = new Color(0, 0, 0, RS_ALPHA);
        Modified.Apply(card.HighlightRectangle);
    }

    internal static void InitHighlightValues(Rectangle rect)
    {
        Original = new HighlightValues(rect.Dashed, rect.CornerRadius, rect.Thickness, rect.Width, rect.Height);

        var delta = RS_THICKNESS - rect.Thickness;
        var width = rect.Width + delta + RS_PADDING;
        var height = rect.Height + delta + RS_PADDING;

        Modified = new HighlightValues(RS_DASHED, RS_CORNER_RADIUS, RS_THICKNESS, width, height);
    }    

    internal class HighlightValues(bool dashes, float cornerRadius, float thickness, float width, float height)
    {
        public bool Dashed { get; } = dashes;
        public float CornerRadius { get; } = cornerRadius;
        public float Thickness { get; } = thickness;
        public float Width { get; } = width;
        public float Height { get; } = height;        

        internal void Apply(Rectangle rect)
        {
            rect.Dashed = Dashed;
            rect.CornerRadius = CornerRadius;
            rect.Thickness = Thickness;
            rect.Width = Width;
            rect.Height = Height;
        }

        internal bool IsApplied(Rectangle rect) =>
            rect.Dashed == Dashed &&
            rect.CornerRadius == CornerRadius &&
            rect.Thickness == Thickness &&
            rect.Height == Height &&
            rect.Width == Width;
    }
}
