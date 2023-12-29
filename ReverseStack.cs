using HarmonyLib;
using ReverseStack.Configuration;
using ReverseStack.Patches;

namespace ReverseStack;

public partial class ReverseStack : Mod
{
    public override void Ready()
    {
        var modConfig = ReverseStackConfig.Init(Config);

        var originalHighlightRect = PrefabManager.instance.GameCardPrefab.HighlightRectangle;
        HighlightReverseStackTargets.Init(modConfig, originalHighlightRect);

        Harmony.PatchAll();
    }
}
