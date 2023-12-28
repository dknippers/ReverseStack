using HarmonyLib;
using ReverseStack.Patches;

namespace ReverseStack;

public class ReverseStack : Mod
{
    public override void Ready()
    {
        var originalHighlightRect = PrefabManager.instance.GameCardPrefab.HighlightRectangle;
        HighlightReverseStackTargets.InitHighlightValues(originalHighlightRect);
        Harmony.PatchAll();
    } 
}
