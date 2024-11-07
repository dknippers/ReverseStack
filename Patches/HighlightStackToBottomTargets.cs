using HarmonyLib;
using Shapes;
using StackToBottom.Configuration;
using StackToBottom.Extensions;
using UnityEngine;

namespace StackToBottom.Patches;

/// <summary>
/// Highlight targets that the currently dragged card can be stacked to the bottom of.
/// For an explanation of what stack to bottom is see <see cref="EnableStackToBottomOnDrag"/>.
/// </summary>
[HarmonyPatch]
public static class HighlightStackToBottomTargets
{
    public static HighlightValues Original = null!;
    public static HighlightValues Modified = null!;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameCard), "Update")]
    public static void GameCard_Update(GameCard __instance)
    {
        if (__instance is not GameCard card || card.HighlightActive) return;

        if (WorldManager.instance.DraggingCard is not GameCard draggingCard ||
            ReferenceEquals(card, draggingCard) ||
            !card.IsRoot() ||
            card.CanHaveOnTop(draggingCard) ||
            !draggingCard.CanStackToBottomOf(card))
        {
            if (!Original.IsApplied(card.HighlightRectangle))
            {
                Original.Apply(card.HighlightRectangle);
            }

            return;
        }

        // StackToBottom highlight
        card.HighlightActive = true;
        card.HighlightRectangle.enabled = true;

        Modified.Apply(card.HighlightRectangle);
    }

    internal static void Init(StackToBottomConfig config, Rectangle rect)
    {
        Original = new HighlightValues(rect.Dashed, rect.CornerRadius, rect.Thickness, rect.Width, rect.Height, rect.Color);

        UpdateModified(config, Original);

        config.OnChange += cfg => UpdateModified(cfg, Original);
    }

    private static void UpdateModified(StackToBottomConfig config, HighlightValues original)
    {
        const float CORNER_RADIUS = 0.01f;
        const float OFFSET = -0.014f;

        var delta = config.HighlightThickness - original.Thickness;
        var width = original.Width + 2 * delta + (config.HighlightDashed ? 0 : OFFSET);
        var height = original.Height + 2 * delta + (config.HighlightDashed ? 0 : OFFSET);

        Modified = new HighlightValues(config.HighlightDashed, CORNER_RADIUS, config.HighlightThickness, width, height, config.HighlightColor);
    }

    public class HighlightValues(bool dashes, float cornerRadius, float thickness, float width, float height, Color color)
    {
        public bool Dashed { get; } = dashes;
        public float CornerRadius { get; } = cornerRadius;
        public float Thickness { get; } = thickness;
        public float Width { get; } = width;
        public float Height { get; } = height;
        public Color Color { get; } = color;

        public void Apply(Rectangle rect)
        {
            if (rect.Dashed != Dashed)
            {
                rect.Dashed = Dashed;
            }

            if (rect.CornerRadius != CornerRadius)
            {
                rect.CornerRadius = CornerRadius;
            }

            if (rect.Thickness != Thickness)
            {
                rect.Thickness = Thickness;
            }

            if (rect.Width != Width)
            {
                rect.Width = Width;
            }

            if (rect.Height != Height)
            {
                rect.Height = Height;
            }

            if (rect.Color != Color)
            {
                rect.Color = Color;
            }
        }

        public bool IsApplied(Rectangle rect) =>
            rect.Dashed == Dashed &&
            rect.CornerRadius == CornerRadius &&
            rect.Thickness == Thickness &&
            rect.Width == Width &&
            rect.Height == Height &&
            rect.Color == Color;
    }
}
