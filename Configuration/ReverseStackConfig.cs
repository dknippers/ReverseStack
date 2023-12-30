using UnityEngine;

namespace ReverseStack.Configuration;

public class ReverseStackConfig
{
    /// <summary>
    /// On change handler, <paramref name="updatedConfig"/> is the same instance as <see cref="OnChange"/> was called on but with updated values.
    /// </summary>
    /// <param name="updatedConfig">The same config instance but with updated values</param>
    public delegate void OnChangeHandler(ReverseStackConfig updatedConfig);

    public static class Defaults
    {
        public const string HIGHLIGHT_COLOR = "#444";
        public const float HIGHLIGHT_ALPHA = 0.8f;
        public const float HIGHLIGHT_THICKNESS = 0.04f;
        public const bool HIGHLIGHT_DASHED = false;
        public const bool ENABLE_FOR_AUTO_STACK = true;
        public const bool AUTO_STACK_PREFER_NORMAL_STACK = false;
        public const float AUTO_STACK_RANGE = 2.0f;
        public static readonly Color HighlightColor = new(0, 0, 0, HIGHLIGHT_ALPHA);
    }

    private ReverseStackConfig() { }

    public static ReverseStackConfig? Instance { get; private set; }

    public Color HighlightColor { get; private set; }
    public float HighlightThickness { get; private set; }
    public bool HighlightDashed { get; private set; }

    public bool EnableForAutoStack { get; private set; }
    public bool AutoStackPreferNormalStack { get; private set; }
    public float AutoStackRange { get; private set; }

    public event OnChangeHandler? OnChange;

    internal static ReverseStackConfig Init(ConfigFile config)
    {
        Instance ??= new ReverseStackConfig();

        var hex = GetValue(config,
            "HighlightColor",
            "Highlight color",
            Defaults.HIGHLIGHT_COLOR,
@$"The highlight color in hex.

Default is {Defaults.HIGHLIGHT_COLOR}");

        var alpha = GetValue(config,
            "HighlightAlpha",
            "Highlight color alpha",
            Defaults.HIGHLIGHT_ALPHA,
@$"The alpha channel of the highlight color.
0 = transparent, 1 = opaque.

Default is {Defaults.HIGHLIGHT_ALPHA}");

        var thickness = GetValue(config,
            "HighlightThickness",
            "Highlight thickness",
            Defaults.HIGHLIGHT_THICKNESS,
@$"The thickness of the highlight.

Default is {Defaults.HIGHLIGHT_THICKNESS}");

        var dashed = GetValue(config,
             "HighlightDashed",
             "Highlight dashed",
             Defaults.HIGHLIGHT_DASHED,
@$"Use animated dashed borders for the highlight (On) or solid borders (Off).

Default is {(Defaults.HIGHLIGHT_DASHED ? "On" : "Off")}");

        var enableForAutoStack = GetValue(config,
             "EnableForAutoStack",
             "Enable for auto stack",
             Defaults.ENABLE_FOR_AUTO_STACK,
@$"Enable Reverse Stack whenever the game attempts to automatically stack a card,
for example when a card is spawned after harvesting among many other situations.

In addition to the default game logic of looking for stacks to stack on top of we
will also check for any Reverse Stack targets and use them if they are closer.

Default is {(Defaults.ENABLE_FOR_AUTO_STACK ? "On" : "Off")}");

        var autoStackPreferNormalStack = GetValue(config,
             "AutoStackPreferNormalStack",
             "Prefer normal stack\nin auto stack",
             Defaults.AUTO_STACK_PREFER_NORMAL_STACK,
@$"If turned On a normal stack always has priority during auto stacking cards,
i.e. if the game finds a valid stack target we will not attempt to find a Reverse Stack
target even if there might be one closer to the card to stack.

Only if the game finds no normal stack target we will look for a Reverse Stack target.

If turned Off we will always look for a Reverse Stack target but only use it
when it is closer than a normal stack target.

Default is {(Defaults.AUTO_STACK_PREFER_NORMAL_STACK ? "On" : "Off")}");

        var autoStackRange = GetValue(config,
             "AutoStackRange",
             "Auto stack range",
             Defaults.AUTO_STACK_RANGE,
@$"The maximum range to consider when looking for auto stack targets.

If you use a mod like Stack Further it is best to set this value to the
same value used in their mod instead of the default.

Default is {Defaults.AUTO_STACK_RANGE}");


        var highlightColor = HexToRgb(hex, alpha);

        Instance.HighlightColor = highlightColor;
        Instance.HighlightThickness = thickness;
        Instance.HighlightDashed = dashed;
        Instance.EnableForAutoStack = enableForAutoStack;
        Instance.AutoStackPreferNormalStack = autoStackPreferNormalStack;
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
            return Defaults.HighlightColor;
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
            return Defaults.HighlightColor;
        }
    }
}
