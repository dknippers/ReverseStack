using UnityEngine;

namespace ReverseStack.Configuration;

public class ReverseStackConfig
{
    private const string DEFAULT_HIGHLIGHT_COLOR = "#000";
    private const float DEFAULT_HIGHLIGHT_ALPHA = 0.8f;
    private const float DEFAULT_HIGHLIGHT_THICKNESS = 0.04f;
    private const bool DEFAULT_HIGHLIGHT_DASHED = false;
    private static readonly Color _defaultColor = new(0, 0, 0, DEFAULT_HIGHLIGHT_ALPHA);

    private ReverseStackConfig() { }

    public static ReverseStackConfig? Instance { get; private set; }

    public Color HighlightColor { get; private set; }
    public float HighlightThickness { get; private set; }
    public bool HighlightDashed { get; private set; }

    public delegate void OnChangeHandler(ReverseStackConfig newConfig);

    public event OnChangeHandler? OnChange;

    internal static ReverseStackConfig Init(ConfigFile config)
    {
        Instance ??= new ReverseStackConfig();

        var hex = GetValue(config,
            "HighlightColor",
            "Highlight color",
            DEFAULT_HIGHLIGHT_COLOR,
            $"The highlight color in hex. Default is black: {DEFAULT_HIGHLIGHT_COLOR}");

        var alpha = GetValue(config,
            "HighlightAlpha",
            "Highlight color alpha",
            DEFAULT_HIGHLIGHT_ALPHA,
            $"The alpha channel of the highlight color, 0 = transparent and 1 = opaque. Default is {DEFAULT_HIGHLIGHT_ALPHA}");

        var thickness = GetValue(config,
            "HighlightThickness",
            "Highlight thickness",
            DEFAULT_HIGHLIGHT_THICKNESS,
            $"The thickness of the highlight. Default is {DEFAULT_HIGHLIGHT_THICKNESS}");

        var dashed = GetValue(config,
             "HighlightDashed",
             "Highlight dashed",
             DEFAULT_HIGHLIGHT_DASHED,
             $"Use animated dashed borders for the highlight (On) or solid borders (Off). Default is {(DEFAULT_HIGHLIGHT_DASHED ? "On" : "Off")}");

        var highlightColor = HexToRgb(hex, alpha);

        Instance.HighlightColor = highlightColor;
        Instance.HighlightThickness = thickness;
        Instance.HighlightDashed = dashed;

        config.OnSave = () =>
        {
            var cfg = Init(config);
            Instance.OnChange?.Invoke(cfg);
        };

        return Instance;
    }

    private static T GetValue<T>(ConfigFile config, string key, string name, T defaultValue, string tooltip)
    {
        return config.GetEntry<T>(key, defaultValue, new ConfigUI
        {
            Name = name,
            Tooltip = tooltip,
            RestartAfterChange = false,
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
