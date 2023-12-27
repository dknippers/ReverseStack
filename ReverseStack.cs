using HarmonyLib;

namespace ReverseStack;

public class ReverseStack : Mod
{
    public override void Ready() => Harmony.PatchAll();    
}
