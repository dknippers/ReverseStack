using UnityEngine;

namespace ReverseStack.Configuration;

public class ReverseStackConfig
{
    /// <summary>
    /// On change handler, <paramref name="updatedConfig"/> is the same instance as <see cref="OnChange"/> was called on but with updated values.
    /// </summary>
    /// <param name="updatedConfig">The same config instance but with updated values</param>
    public delegate void OnChangeHandler(ReverseStackConfig updatedConfig);

    private const string DEFAULT_HIGHLIGHT_COLOR = "#000";
    private const float DEFAULT_HIGHLIGHT_ALPHA = 0.8f;
    private const float DEFAULT_HIGHLIGHT_THICKNESS = 0.04f;
    private const bool DEFAULT_HIGHLIGHT_DASHED = false;
    private const bool DEFAULT_ENABLE_FOR_AUTO_STACK = true;
    private const float DEFAULT_AUTO_STACK_RANGE = 2.0f;

    private static readonly Color _defaultColor = new(0, 0, 0, DEFAULT_HIGHLIGHT_ALPHA);

    private ReverseStackConfig() { }

    public static ReverseStackConfig? Instance { get; private set; }

    public Color HighlightColor { get; private set; }
    public float HighlightThickness { get; private set; }
    public bool HighlightDashed { get; private set; }

    public bool EnableForAutoStack { get; private set; }
    public float AutoStackRange { get; private set; }

    public event OnChangeHandler? OnChange;

    internal static ReverseStackConfig Init(ConfigFile config)
    {
        Instance ??= new ReverseStackConfig();

        var hex = GetValue(config,
            "HighlightColor",
            "Highlight color",
            DEFAULT_HIGHLIGHT_COLOR,
@$"The highlight color in hex.

Default is {DEFAULT_HIGHLIGHT_COLOR}");

        var alpha = GetValue(config,
            "HighlightAlpha",
            "Highlight color alpha",
            DEFAULT_HIGHLIGHT_ALPHA,
@$"The alpha channel of the highlight color.
0 = transparent, 1 = opaque.

Default is {DEFAULT_HIGHLIGHT_ALPHA}");

        var thickness = GetValue(config,
            "HighlightThickness",
            "Highlight thickness",
            DEFAULT_HIGHLIGHT_THICKNESS,
@$"The thickness of the highlight.

Default is {DEFAULT_HIGHLIGHT_THICKNESS}");

        var dashed = GetValue(config,
             "HighlightDashed",
             "Highlight dashed",
             DEFAULT_HIGHLIGHT_DASHED,
@$"Use animated dashed borders for the highlight (On) or solid borders (Off).

Default is {(DEFAULT_HIGHLIGHT_DASHED ? "On" : "Off")}");

        var enableForAutoStack = GetValue(config,
             "EnableForAutoStack",
             "Enable for auto stack",
             DEFAULT_ENABLE_FOR_AUTO_STACK,
@$"Enable Reverse Stack whenever the game attempts to automatically stack a card,
for example when a card is spawned after harvesting.

If the game cannot stack a card to the top of nearby stack in such scenarios we
will attempt to stack it to the bottom instead.

Default is {(DEFAULT_ENABLE_FOR_AUTO_STACK ? "On" : "Off")}");

        var autoStackRange = GetValue(config,
             "AutoStackRange",
             "Auto stack range",
             DEFAULT_AUTO_STACK_RANGE,
@$"The maximum range to consider when looking for auto stack targets.

If you use a mod like Stack Further it is best to set this value to the
same value used in their mod instead of the default.

Default is {DEFAULT_AUTO_STACK_RANGE}");

        var highlightColor = HexToRgb(hex, alpha);

        Instance.HighlightColor = highlightColor;
        Instance.HighlightThickness = thickness;
        Instance.HighlightDashed = dashed;
        Instance.EnableForAutoStack = enableForAutoStack;
        Instance.AutoStackRange = autoStackRange;

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
