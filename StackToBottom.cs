using HarmonyLib;
using StackToBottom.Configuration;
using StackToBottom.Patches;

namespace StackToBottom;

public partial class StackToBottom : Mod
{
    public override void Ready()
    {
        var modConfig = StackToBottomConfig.Init(Config);

        var originalHighlightRect = PrefabManager.instance.GameCardPrefab.HighlightRectangle;
        HighlightStackToBottomTargets.Init(modConfig, originalHighlightRect);

        Harmony.PatchAll();
    }
}
