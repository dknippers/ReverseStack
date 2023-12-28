using UnityEngine;

namespace ReverseStack.Configuration;

public class ReverseStackConfig
{
    private const string DEFAULT_HIGHLIGHT_COLOR = "#000";
    private const float DEFAULT_HIGHLIGHT_ALPHA = 0.8f;
    private static readonly Color _defaultColor = new(0, 0, 0, DEFAULT_HIGHLIGHT_ALPHA);

    private ReverseStackConfig(Color highlightColor)
    {
        HighlightColor = highlightColor;
    }

    public static ReverseStackConfig Instance { get; private set; } = null!;

    public Color HighlightColor { get; }

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

        var highlightColor = HexToRgb(hex, alpha);

        return Instance = new ReverseStackConfig(highlightColor);
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
        if (hex.Length != 7 && hex.Length != 4 && hex[0] != '#')
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
