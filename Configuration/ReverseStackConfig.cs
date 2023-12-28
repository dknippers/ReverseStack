using UnityEngine;

namespace ReverseStack.Configuration;

public class ReverseStackConfig
{
    private const string DEFAULT_HIGHLIGHT_COLOR = "#000";
    private const float DEFAULT_HIGHLIGHT_ALPHA = 0.8f;
    private const bool DEFAULT_HIGHLIGHT_DASHED = false;
    private const float DEFAULT_HIGHLIGHT_THICKNESS = 0.036f;
    private static readonly Color _defaultColor = new(0, 0, 0, DEFAULT_HIGHLIGHT_ALPHA);

    private ReverseStackConfig(Color highlightColor, bool highlightDashed, float highlightThickness)
    {
        HighlightColor = highlightColor;
        HighlightDashed = highlightDashed;
        HighlightThickness = highlightThickness;
    }

    public static ReverseStackConfig Instance { get; private set; } = null!;

    public Color HighlightColor { get; }
    public bool HighlightDashed { get; }
    public float HighlightThickness { get; }

    internal static ReverseStackConfig Init(ConfigFile config)
    {
        if (Instance is not null)
        {
            throw new InvalidOperationException("Configuration already initialized");
        }

        var hex = GetValue(config,
            "HighlightColor",
            "Highlight color",
            DEFAULT_HIGHLIGHT_COLOR,
            $"The highlight color (hex notation) for Reverse Stack targets. Default is black: {DEFAULT_HIGHLIGHT_COLOR}");

        var alpha = GetValue(config,
            "HighlightAlpha",
            "Highlight color alpha",
            DEFAULT_HIGHLIGHT_ALPHA,
            $"The alpha channel of the highlight color, 0 = transparent and 1 = opaque. Default is {DEFAULT_HIGHLIGHT_ALPHA}");

        var dashed = GetValue(config,
            "HighlightDashed",
            "Highlight dashed",
            DEFAULT_HIGHLIGHT_DASHED,
            $"Use dashes for the Reverse Stack highlight. Default is {(DEFAULT_HIGHLIGHT_DASHED ? "On" : "Off")}");

        var thickness = GetValue(config,
            "HighlightThickness",
            "Highlight thickness",
            DEFAULT_HIGHLIGHT_THICKNESS,
            $"The thickness of the highlight. Default is {DEFAULT_HIGHLIGHT_THICKNESS}");

        var highlightColor = HexToRgb(hex, alpha);

        return Instance = new ReverseStackConfig(highlightColor, dashed, thickness);
    }

    private static T GetValue<T>(ConfigFile config, string key, string name, T defaultValue, string tooltip)
    {
        return config.GetEntry<T>(key, defaultValue, new ConfigUI
        {
            Name = name,
            Tooltip = tooltip,
            RestartAfterChange = true,
        }).Value;
    }

    private static Color HexToRgb(string hex, float alpha = 1f)
    {
        if (hex.Length == 0 || (hex.Length != 7 && hex.Length != 4) || hex[0] != '#')
        {
            return _defaultColor;
        }

        if (hex.Length == 4)
        {
            // Shorthand syntax
            hex = $"#{hex[1]}{hex[1]}{hex[2]}{hex[2]}{hex[3]}{hex[3]}";
        }

        try
        {
            var red = Convert.ToInt32(hex.Substring(1, 2), 16) / 255f;
            var green = Convert.ToInt32(hex.Substring(3, 2), 16) / 255f;
            var blue = Convert.ToInt32(hex.Substring(5, 2), 16) / 255f;

            return new Color(red, green, blue, alpha);
        }
        catch
        {
            return _defaultColor;
        }
    }
}
